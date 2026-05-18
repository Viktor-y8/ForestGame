using UnityEditor.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class Soil : MonoBehaviour
{
    public SoilType type;
    
    private TileObject currentObject;
    public GameObject treePrefab;
    
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

    public bool HasObject => currentObject != null;

    public TileObject CurrentObject => currentObject;

    private void Start()
    {
        TimeManager.Instance.OnDayPassed += UpdateMoisture;
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

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnDayPassed -= UpdateMoisture;
        }
    }
}
