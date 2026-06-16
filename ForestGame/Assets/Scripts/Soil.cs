using UnityEditor.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class Soil : MonoBehaviour
{
    public SoilType type;
    
    private TileObject currentObject;
    public GameObject treePrefab;
    public GameObject ditchPrefab;

    public event System.Action<SoilType> OnSoilChanged;
    
    public int x;
    public int y;
    public Grid grid;

    [Range(0f, 1f)]
    public float moisture = 0.5f;

    public float moistureRetention = 1f;

    public float fertility = 0.5f;

    [Range(0f, 1f)]
    public float shade = 0f;

    public bool isOnFire = false;
    [Range(0f, 1f)]
    public float burnProgress = 0f;               // 0–1, tree dies at 1
    private const float burnRate = 0.12f;
    private bool recentlyOnFire = false;
    private int recentlyOnFireCounter = 0;

    public void RecentFireCountUp() 
    { 
        if(recentlyOnFire) recentlyOnFireCounter++;
        if(recentlyOnFireCounter >= 30) recentlyOnFire = false;
    }
    public bool HasObject => currentObject != null;

    public TileObject CurrentObject => currentObject;

    private void Start()
    {
        TimeManager.Instance.OnDayPassed += UpdateMoisture;
        TimeManager.Instance.OnDayPassed += RecentFireCountUp;
    }

    private void UpdateMoisture()
    {
        Tree tree = CurrentObject is Tree treeObj ? treeObj : null;

        if (tree != null && tree.isMature)
        {
            moisture += 0.001f;
        }

        WeatherType weather = WeatherManager.Instance.currentWeather;

        float change = 0f;

        switch (weather)
        {
            case WeatherType.Rain:
                change = 0.015f;
                break;

            case WeatherType.Drought:
                change = -0.008f;
                break;

            case WeatherType.Heatwave:
                change = -0.015f;
                break;

            case WeatherType.Normal:
                change = -0.002f;
                break;
        }

        moisture += change * moistureRetention;

        int nearbyTrees = 0;

        foreach (Soil s in grid.Adjacent(this))
        {
            if (s.CurrentObject is Tree treeAdj && treeAdj.isMature)
            {
                nearbyTrees++;
            }
        }

        moisture += nearbyTrees * 0.0005f;


        moisture = Mathf.Clamp01(moisture);
    }

    public void Water(float amount)
    {
        float effectiveAmount = amount * moistureRetention;
        moisture = Mathf.Clamp01(moisture + effectiveAmount);
    }

    public void UpdateShade()
    {
        shade = 0f;

        Soil[] neighbors = grid.Adjacent(this);

        foreach (Soil s in neighbors)
        {
            if (s == null || !s.HasObject) continue;

            Tree tree = s.CurrentObject as Tree;

            if (tree == null) continue;

            if (tree.isMature)
            {
                shade += 0.2f;
            }
            else if(tree.AgeYears >= tree.data.saplingAge)
            {
                shade += 0.1f;
            }
        }

        shade = Mathf.Clamp01(shade);
    }
    
    public void RefreshLocalEnvironment()
    {
        UpdateShade();
    }

    public void PlantTree(TreeData treeData)
    {

        if (HasObject) return;

        GameObject treeObj = Instantiate(treePrefab, transform.position, Quaternion.identity);

        Tree tree = treeObj.GetComponent<Tree>();

        tree.Initialize(this, treeData);

        currentObject = tree;

        grid.RefreshNeighbors(this);
    }
    
    public void PlantDitch()
    {
        if (HasObject) return;

        GameObject obj = Instantiate(ditchPrefab, transform.position, Quaternion.identity);
        Ditch ditch = obj.GetComponent<Ditch>();
        ditch.Initialize(this);
        currentObject = ditch;
    }

    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        InteractionManager.Instance.Interact(this);
    }

    public void ChangeSoil(SoilType newType)
    {
        type = newType;
        OnSoilChanged?.Invoke(type);
    }
    public bool RemoveObject()
    {
        if (currentObject == null) return false;

        bool shouldReturnSeed = currentObject is Tree tree && tree.justPlanted;

        Destroy(currentObject.gameObject);

        currentObject = null;

        grid.RefreshNeighbors(this);

        return shouldReturnSeed;
    }

    public void Ignite()
    {
        if (isOnFire || moisture > 0.6f || recentlyOnFire) return;  // wet soil won't ignite
        if (CurrentObject is Ditch) return;

        isOnFire = true;
        burnProgress = 0f;
    }

    public void Extinguish()
    {
        isOnFire = false;
        burnProgress = 0f;
    }

    public void UpdateFire()
    {
        if(moisture >= 0.8) Extinguish();

        if (!isOnFire) return;

        // Dry soil and high shade (dense canopy) burn faster
        float rate = burnRate;
        rate *= (1f - moisture);
        rate *= (1f + shade * 0.5f);

        if (!HasObject) rate *= 1.2f;

        burnProgress += rate;

        if (burnProgress >= 1f)
        {
            if (HasObject) RemoveObject();
            Extinguish();
            recentlyOnFire = true;
            recentlyOnFireCounter = 0;

            // Burnt soil loses moisture retention temporarily
            //moistureRetention = Mathf.Max(0.3f, moistureRetention - 0.2f);
        }
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnDayPassed -= UpdateMoisture;
        }
    }
}
