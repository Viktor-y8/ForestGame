using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TreeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TreeData treeData;

    private RectTransform buttonContainer;

    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private Button button;

    private void Awake()
    {
        buttonContainer = transform.parent.GetComponent<RectTransform>();
    }

    private void Start()
    {
        RefreshCountText();
        CheckIfRequired();
    }

    public void OnClick()
    {
        if (!IsTreeRequired())
            return;

        SoundManager.Instance.PlaySFX("buttonSFX");
        InteractionManager.Instance.SelectPlantTool(treeData);
    }

    private void RefreshCountText()
    {
        if (buttonText == null) return;

        buttonText.text = treeData.treeName;
    }

    private bool IsTreeRequired()
    {
        if (LevelManager.Instance == null || LevelManager.Instance.CurrentLevel == null)
            return false;

        foreach (LevelData.TreeRequirement requirement in
                 LevelManager.Instance.CurrentLevel.treeRequirements)
        {
            if (requirement.treeType == treeData &&
                requirement.requiredMatureCount > 0)
            {
                return true;
            }
        }

        return false;
    }

    private void CheckIfRequired()
    {
        bool required = IsTreeRequired();

        if (button != null)
            button.interactable = required;
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