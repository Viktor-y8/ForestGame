using UnityEngine;

public enum ToolType
{
    None,
    Plant,
    RemoveTree,
    Fertilize,
    ReplaceSoil,
    Water
}

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance;

    private TreeData selectedTree;
    private ToolType currTool;

    [SerializeField] private GameObject cursorPreview;
    private SpriteRenderer previewRenderer;

    private Grid grid;

    public int seedCount;

    public int waterBudget = 10;
    private const float waterAmount = 0.3f;

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

        TimeManager.Instance.OnMonthPassed += RefillWaterBudget;
    }

    private void Update()
    {
        if (grid == null) return;

        if (selectedTree != null)
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
                if (soil.HasObject)
                    previewRenderer.color = new Color(1, 0, 0, 0.75f);
                else
                    previewRenderer.color = new Color(1, 1, 1, 0.75f);
            }
            //else cursorPreview.SetActive(false);
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
        cursorPreview.SetActive(false);
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
                TryWater(soil);
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

    private void RefillWaterBudget()
    {
        int refill = WeatherManager.Instance.currentWeather == WeatherType.Rain ? 8 : 4;
        waterBudget = Mathf.Min(waterBudget + refill, 20);
    }

    public void TryRemove(Soil soil)
    {

        if (!soil.HasObject) return;

        if(soil.RemoveObject()) seedCount++;
    }


    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.OnMonthPassed -= RefillWaterBudget;
    }
}
