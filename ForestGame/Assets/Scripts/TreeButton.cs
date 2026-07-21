using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TreeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TreeData treeData;

    private RectTransform buttonContainer;

    [SerializeField] private TMP_Text buttonText;

    private void Awake()
    {
        buttonContainer = transform.parent.GetComponent<RectTransform>();
    }

    public void OnClick()
    {
        SoundManager.Instance.PlaySFX("buttonSFX");
        InteractionManager.Instance.SelectPlantTool(treeData);
    }

    private void Start()
    {
        RefreshCountText();
        InteractionManager.OnSeedChanged += RefreshCountText;
    }

    private void OnDestroy()
    {
        InteractionManager.OnSeedChanged -= RefreshCountText;
    }

    private void RefreshCountText()
    {
        if (buttonText == null) return;

        buttonText.text = treeData.treeName + " - " + InteractionManager.Instance.seedCount;
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