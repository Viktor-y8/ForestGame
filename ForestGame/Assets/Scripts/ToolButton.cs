using UnityEngine;
using UnityEngine.EventSystems;

public class ToolButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private ToolType tool;
    [SerializeField] private string toolName;
    [TextArea][SerializeField] private string toolDescription;

    // Drag the tool buttons group parent here in the inspector
    [SerializeField] private RectTransform buttonContainer;

    private void Awake()
    {
        buttonContainer = transform.parent.GetComponent<RectTransform>();
    }

    public void OnClick()
    {
        InteractionManager.Instance.SelectTool(tool);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TreeTooltipUI.Instance == null) return;
        TreeTooltipUI.Instance.Show(toolName, toolDescription, buttonContainer);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TreeTooltipUI.Instance == null) return;
        TreeTooltipUI.Instance.Hide();
    }
}