using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToolButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private ToolType tool;
    [SerializeField] private string toolName;
    [TextArea][SerializeField] private string toolDescription;

    [SerializeField] private RectTransform buttonContainer;
    [SerializeField] private bool useOwnPosition = false;
    [SerializeField] private TMP_Text toolText;

    private void Awake()
    {
        if (useOwnPosition)
            buttonContainer = GetComponent<RectTransform>();
        else
            buttonContainer = transform.parent.GetComponent<RectTransform>()
                              ?? GetComponent<RectTransform>();
    }

    private void Start()
    {
        RefreshCountText();
        InteractionManager.OnBudgetChanged += RefreshCountText;
    }

    private void OnDestroy()
    {
        InteractionManager.OnBudgetChanged -= RefreshCountText;
    }

    private void RefreshCountText()
    {
        if (toolText == null) return;

        if (tool == ToolType.Fertilize)
            toolText.text = toolName + " - " + InteractionManager.Instance.fertilizeBudget + " / " + InteractionManager.Instance.getMaxFertilizeBudget();
        else if (tool == ToolType.Water)
            toolText.text = toolName + " - " + InteractionManager.Instance.waterBudget + " / " + InteractionManager.Instance.getMaxWaterBudget();
    }

    public void OnClick()
    {
        SoundManager.Instance.PlaySFX("buttonSFX"); InteractionManager.Instance.SelectTool(tool);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TreeTooltipUI.Instance == null) return;
        TreeTooltipUI.Instance.Show(toolName, toolDescription, buttonContainer, useOwnPosition);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TreeTooltipUI.Instance == null) return;
        TreeTooltipUI.Instance.Hide();
    }
}