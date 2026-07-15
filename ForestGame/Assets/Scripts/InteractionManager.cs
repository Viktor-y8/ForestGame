using UnityEngine;

public enum ToolType
{
    None,
    Plant,
    RemoveTree,
    Fertilize,
    ReplaceSoil,
    Water,
    Ditch
}

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance;

    private TreeData selectedTree;
    private ToolType currTool;

    [SerializeField] private GameObject cursorPreview;
    private SpriteRenderer previewRenderer;


    [Header("Tool Sprites")]
    [SerializeField] private Sprite waterSprite;
    [SerializeField] private Sprite ditchSprite;
    [SerializeField] private Sprite removeSprite;


    private Grid grid;

    public int seedCount;

    public int waterBudget = 10;
    private const float waterAmount = 0.3f;

    public int ditchBudget = 5;

    private void Awake()
    {
        Instance = this;
        previewRenderer = cursorPreview.GetComponent<SpriteRenderer>();
        previewRenderer.color = new Color(1, 1, 1, 0.75f);
        cursorPreview.SetActive(false);
        currTool = ToolType.None;
    }

    private void Start()
    {

        TimeManager.Instance.OnMonthPassed += RefillToolBudget;
    }

    private void Update()
    {
        if (grid == null) return;

        bool hasActivePreview = selectedTree != null ||
                                currTool == ToolType.Water ||
                                currTool == ToolType.Ditch ||
                                currTool == ToolType.RemoveTree;

        if (hasActivePreview)
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 0f;

            int x, y;
            grid.GetXY(pos, out x, out y);
            Soil soil = grid.GetValue(x, y);
            Vector3 snapped = grid.GetSnappedPosition(pos);

            if (soil != null)
            {
                cursorPreview.transform.position = snapped;
                cursorPreview.SetActive(true);

                // Red tint when action isn't valid for this tile
                bool invalid = currTool switch
                {
                    ToolType.Plant => soil.HasObject,
                    ToolType.RemoveTree => !soil.HasObject || soil.isLocked,
                    ToolType.Ditch => soil.isLocked || soil.isOnFire || soil.CurrentObject is Ditch,
                    ToolType.Water => soil.isLocked,
                    _ => false
                };

                previewRenderer.color = invalid
                    ? new Color(1, 0, 0, 0.75f)
                    : new Color(1, 1, 1, 0.75f);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            selectedTree = null;
            cursorPreview.SetActive(false);
            currTool = ToolType.None;
            InfoPanelUI.Instance.Hide();
        }
    }

    public void SelectPlantTool(TreeData treeData)
    {
        currTool = ToolType.Plant;
        selectedTree = treeData;

        previewRenderer.sprite = treeData.previewSprite;
        cursorPreview.transform.position = Input.mousePosition;

        cursorPreview.SetActive(true);
    }

    public void SelectTool(ToolType tool)
    {
        currTool = tool;
        selectedTree = null;

        Sprite toolSprite = tool switch
        {
            ToolType.Water => waterSprite,
            ToolType.Ditch => ditchSprite,
            ToolType.RemoveTree => removeSprite,
            _ => null
        };

        if (toolSprite != null)
        {
            previewRenderer.sprite = toolSprite;
            previewRenderer.color = new Color(1, 1, 1, 0.75f);
            cursorPreview.SetActive(true);
        }
        else
        {
            cursorPreview.SetActive(false);
        }
    }

    public void Interact(Soil soil)
    {
        switch (currTool)
        {
            case ToolType.None:
                InfoPanelUI.Instance.Show(soil);
                break;

            case ToolType.Plant:
                if (selectedTree != null)
                    TryPlant(soil);
                break;

            case ToolType.RemoveTree:
                TryRemove(soil);
                break;

            case ToolType.Fertilize:
                soil.ChangeSoil(SoilType.Fertile);
                break;

            case ToolType.ReplaceSoil:
                soil.ChangeSoil(SoilType.Rocky);
                break;


            case ToolType.Water:
                //FireManager.Instance.StartFire(soil);
                TryWater(soil);
                break;

            case ToolType.Ditch:
                TryDigDitch(soil);
                break;
        }
    }

    public void SetGrid(Grid grid)
    {
        this.grid = grid;
    }

    public void SelectTree(TreeData treeData)
    {
        selectedTree = treeData;

        previewRenderer.sprite = treeData.previewSprite;
        cursorPreview.transform.position = Input.mousePosition;

        cursorPreview.SetActive(true);
    }

    public void TryPlant(Soil soil)
    {

        if (selectedTree == null || soil.HasObject || seedCount <= 0) return;

        soil.PlantTree(selectedTree);
        seedCount--;
    }

    public void TryWater(Soil soil)
    {
        if (waterBudget <= 0) return;

        soil.Water(waterAmount);
        waterBudget--;
    }

    public void TryDigDitch(Soil soil)
    {
        if (ditchBudget <= 0) return;
        if (soil.isOnFire) return;
        if (soil.CurrentObject is Ditch) return;    // already a ditch

        if (soil.RemoveObject()) seedCount++;

        soil.PlantDitch();
        ditchBudget--;
    }

    private void RefillToolBudget()
    {
        int refill = WeatherManager.Instance.currentWeather == WeatherType.Rain ? 8 : 4;
        waterBudget = Mathf.Min(waterBudget + refill, 20);

        ditchBudget += Mathf.Min(ditchBudget + 5, 25);
    }

    public void TryRemove(Soil soil)
    {

        if (!soil.HasObject) return;

        if(soil.RemoveObject()) seedCount++;
    }


    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.OnMonthPassed -= RefillToolBudget;
    }
}
