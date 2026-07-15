using UnityEngine;
using UnityEngine.EventSystems;

public class TreeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TreeData treeData;

    private RectTransform buttonContainer;

    private void Awake()
    {
        // Automatically use whatever parent holds the buttons
        buttonContainer = transform.parent.GetComponent<RectTransform>();
    }

    public void OnClick()
    {
        InteractionManager.Instance.SelectPlantTool(treeData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TreeTooltipUI.Instance == null) return;
        TreeTooltipUI.Instance.Show(treeData, buttonContainer);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TreeTooltipUI.Instance == null) return;
        TreeTooltipUI.Instance.Hide();
    }
}