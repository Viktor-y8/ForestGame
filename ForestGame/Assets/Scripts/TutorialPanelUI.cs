using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Image illustration;
    [SerializeField] private Button closeButton;

    private Action onClosed;

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
        panelRoot.SetActive(false);
    }

    public void Show(TutorialStep step, Action onClosedCallback)
    {
        titleText.text = step.title;
        bodyText.text = step.body;

        if (step.image != null)
        {
            illustration.sprite = step.image;
            illustration.gameObject.SetActive(true);
        }
        else
        {
            illustration.gameObject.SetActive(false);
        }

        onClosed = onClosedCallback;
        panelRoot.SetActive(true);

        TimeManager.Instance.scaleTime(0f);
    }

    private void Close()
    {
        panelRoot.SetActive(false);
        TimeManager.Instance.scaleTime(1f);
        onClosed?.Invoke();
    }
}