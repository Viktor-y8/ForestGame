using UnityEngine;

public class PlantManager : MonoBehaviour
{
    public static PlantManager Instance;

    private TreeData selectedTree;

    private void Awake()
    {
        Instance = this;
    }

    public void SelectTree(TreeData treeData)
    {
        selectedTree = treeData;
        Debug.Log("Selected" + treeData.name);
    }

    public void TryPlant(Soil soil)
    {

        if (selectedTree == null) return;

        soil.PlantTree(selectedTree);
    }

    [SerializeField] private GameObject previewObject;

    private void Update()
    {
        if (selectedTree == null) return;

        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0f;

        previewObject.transform.position = pos;
    }
}
