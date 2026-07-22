using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TreeTooltipUI : MonoBehaviour
{
    public static TreeTooltipUI Instance;

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text statsText;

    [SerializeField] private float fixedScreenX = 500f;
    [SerializeField] private float fixedScreenY = 640f;

    [SerializeField] private RectTransform panelRect;

    [SerializeField] private Canvas canvas;

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    private void Display(RectTransform container, bool useOwnPosition = false)
    {
        panel.SetActive(true);
        Canvas.ForceUpdateCanvases();

        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        Vector2 screenPoint;
        Vector3[] ownCorners = null;

        if (!useOwnPosition)
        {
            float totalY = 0f;
            int count = 0;

            foreach (RectTransform child in container)
            {
                if (!child.gameObject.activeInHierarchy) continue;
                Vector3[] corners = new Vector3[4];
                child.GetWorldCorners(corners);
                totalY += (corners[0].y + corners[1].y) / 2f;
                count++;
            }

            float centerY = count > 0 ? totalY / count : Screen.height / 2f;
            screenPoint = new Vector2(fixedScreenX, centerY);
        }
        else
        {
            ownCorners = new Vector3[4];
            container.GetWorldCorners(ownCorners);

            float centerY = (ownCorners[0].y + ownCorners[1].y) / 2f;
            float actualPanelWidth = panelRect.rect.width;
            float gap = 320f;

            float buttonCenterXScreen = (ownCorners[0].x + ownCorners[2].x) / 2f;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, new Vector2(buttonCenterXScreen, 0f), uiCamera, out Vector2 buttonCenterLocal);

            bool isOnRightHalf = buttonCenterLocal.x > 0f;

            float targetX = isOnRightHalf
                ? ownCorners[0].x - actualPanelWidth / 2f - gap
                : ownCorners[2].x + actualPanelWidth / 2f + gap;

            screenPoint = new Vector2(targetX, centerY);
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPoint, uiCamera, out Vector2 localPoint
        );

        localPoint = ClampToCanvas(localPoint, ownCorners);

        panelRect.anchoredPosition = localPoint;
    }

    private Vector2 ClampToCanvas(Vector2 desiredPosition, Vector3[] buttonCorners)
    {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        float panelHalfWidth = panelRect.rect.width / 2f;
        float panelHalfHeight = panelRect.rect.height / 2f;

        float canvasHalfWidth = canvasRect.rect.width / 2f;
        float canvasHalfHeight = canvasRect.rect.height / 2f;

        float screenPadding = 20f;

        float minX = -canvasHalfWidth + panelHalfWidth + screenPadding;
        float maxX = canvasHalfWidth - panelHalfWidth - screenPadding;
        float minY = -canvasHalfHeight + panelHalfHeight + screenPadding;
        float maxY = canvasHalfHeight - panelHalfHeight - screenPadding;

        desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);

        if (buttonCorners == null)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            return desiredPosition;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, buttonCorners[0], uiCamera, out Vector2 buttonLeftLocal);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, buttonCorners[2], uiCamera, out Vector2 buttonRightLocal);

        bool isPlacedRightOfButton = desiredPosition.x > buttonRightLocal.x;

        if (isPlacedRightOfButton)
        {
            float hardMinX = buttonRightLocal.x + screenPadding;
            desiredPosition.x = Mathf.Max(desiredPosition.x, hardMinX);
            desiredPosition.x = Mathf.Min(desiredPosition.x, maxX);
        }
        else
        {
            float hardMaxX = buttonLeftLocal.x - screenPadding;
            desiredPosition.x = Mathf.Min(desiredPosition.x, hardMaxX);
            desiredPosition.x = Mathf.Max(desiredPosition.x, minX);
        }

        return desiredPosition;
    }

    public void Show(TreeData data, RectTransform container, bool useOwnPosition = false)
    {
        nameText.text = data.treeName;
        statsText.text =
            $"Matures in: {data.minMaturityAgeYears}–{data.maxMaturityAgeYears} years\n" +
            $"Max age: {data.maxAgeYears} years\n" +
            $"Drought resistance: {(data.droughtResistance * 100f):0}%\n" +
            $"Shade tolerance: {(data.shadeTolerance * 100f):0}%\n" +
            $"Moisture usage: {(data.moistureUsage * 100f):0}%\n" +
            $"Spread chance: {(data.spreadChance * 100f):0}%\n";

        Display(container, useOwnPosition);
    }

    public void Show(string title, string description, RectTransform container, bool useOwnPosition = false)
    {
        nameText.text = title;
        statsText.text = description;
        Display(container, useOwnPosition);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}