using UnityEngine;

public class TreeButton : MonoBehaviour
{
    public TreeData treeData;

    public void OnClick()
    {
        InteractionManager.Instance.SelectPlantTool(treeData);
    }
}
