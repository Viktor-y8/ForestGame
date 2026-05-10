using UnityEngine;

public class ToolButton : MonoBehaviour
{

    public ToolType tool;
    public void OnClick()
    {
        InteractionManager.Instance.SelectTool(tool);
    }

}
