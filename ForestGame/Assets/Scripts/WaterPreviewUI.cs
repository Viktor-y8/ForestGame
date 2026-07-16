using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WaterPreviewUI : MonoBehaviour
{
    public static WaterPreviewUI Instance;

    [SerializeField] private GameObject barRoot;
    [SerializeField] private Image currentFill;     
    [SerializeField] private Image previewFill;      
    [SerializeField] private RectTransform fireThresholdMarker; 
    [SerializeField] private RectTransform barBackground;       
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image fireIconImage; 

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Show(Soil soil, float waterAmount, Vector3 worldPosition)
    {
        barRoot.SetActive(true);

        Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            screenPoint,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localPoint
        );
        ((RectTransform)barRoot.transform).anchoredPosition = localPoint;

        float current = soil.moisture;
        float projected = Mathf.Clamp01(current + waterAmount * soil.moistureRetention);

        currentFill.fillAmount = current;
        previewFill.fillAmount = projected;

        float barWidth = barBackground.rect.width;
        Vector2 markerPos = fireThresholdMarker.anchoredPosition;
        markerPos.x = barWidth * 0.8f;
        fireThresholdMarker.anchoredPosition = markerPos;

        fireIconImage.color =
        (projected >= 0.8f && current < 0.8f) ? Color.red : Color.white;
    }

    public void Hide()
    {
        barRoot.SetActive(false);
    }
}