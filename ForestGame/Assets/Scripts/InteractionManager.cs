using UnityEngine;

public enum ToolType
{
    None,
    Plant,
    RemoveTree,
    Fertilize,
    ReplaceSoil
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

    private void Awake()
    {
        Instance = this;
        previewRenderer = cursorPreview.GetComponent<SpriteRenderer>();
        previewRenderer.color = new Color(1, 1, 1, 0.75f);
        cursorPreview.SetActive(false);
        currTool = ToolType.None;
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
                if (soil.HasTree)
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
                soil.ChangeSoil(SoilType.Normal);
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

        if (selectedTree == null || soil.HasTree || seedCount <= 0) return;

        soil.PlantTree(selectedTree);
        seedCount--;
    }

    public void TryRemove(Soil soil)
    {

        if (!soil.HasTree) return;

        if(soil.RemoveTree()) seedCount++;
    }

}
