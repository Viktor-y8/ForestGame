using UnityEditor.UI;
using UnityEngine;

public class Soil : MonoBehaviour
{
    public SoilType type;
    private Tree tree;
    public GameObject treePrefab;
    public event System.Action<SoilType> OnSoilChanged;
    public int x;
    public int y;
    public Grid grid;

    public bool HasTree => tree != null;

    public void PlantTree(TreeData treeData)
    {

        if (HasTree) return;

        GameObject treeObj = Instantiate(treePrefab, transform.position, Quaternion.identity);

        tree = treeObj.GetComponent<Tree>();
        tree.Initialize(this, treeData);

    }

    void OnMouseDown()
    {
        InteractionManager.Instance.Interact(this);
    }

    public void ChangeSoil(SoilType newType)
    {
        type = newType;
        OnSoilChanged?.Invoke(type);
    }
    public bool RemoveTree()
    {
        if (!HasTree) return false;

        bool shouldReturnSeed = tree.justPlanted;

        Destroy(tree.gameObject);
        tree = null;

        return shouldReturnSeed;
    }
}
