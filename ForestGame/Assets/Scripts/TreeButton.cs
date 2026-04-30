using UnityEngine;

public class TreeButton : MonoBehaviour
{
    public TreeData treeData;

    public void OnClick()
    {
        PlantManager.Instance.SelectTree(treeData);
    }
}
