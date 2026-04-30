using UnityEngine;

public class Testing : MonoBehaviour
{

    public GameObject myPrefab;
    public GameObject myPrefab2;
    private Grid grid;

    void Start()
    {
        grid = new Grid(4, 2, myPrefab, new Vector3(0, 0));
    }

    private void Update()
    {

       /*if(Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0f;

            TreeData treeData = ScriptableObject.CreateInstance<TreeData>();
            treeData.name = "Test";
            treeData.growthSpeed = 1f;
            treeData.preferredSoil = SoilType.Normal;
            treeData.prefab = myPrefab2;

            grid.SetValue(worldPos, treeData);
        }*/
    }
}
