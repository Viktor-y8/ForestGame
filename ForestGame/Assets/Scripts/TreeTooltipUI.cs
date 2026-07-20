using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TreeTooltipUI : MonoBehaviour
{
    public static TreeTooltipUI Instance;

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text statsText;

    // Fixed screen position — set in inspector to sit next to your button column
    [SerializeField] private float fixedScreenX = 500f;
    [SerializeField] private float fixedScreenY = 640f;

    [SerializeField] private RectTransform panelRect;

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    private void Display(RectTransform container)
    {

        if (TutorialManager.IsTutorialActive) return;

        panel.SetActive(true);
        Canvas.ForceUpdateCanvases();

        float totalY = 0f;
        int count = 0;

        foreach (RectTransform child in container)
        {
            if (!child.gameObject.activeInHierarchy || child.GetComponent<TMPro.TMP_Text>() != null) continue;

            Vector3[] corners = new Vector3[4];
            child.GetWorldCorners(corners);
            totalY += (corners[0].y + corners[1].y) / 2f;
            count++;
        }

        float centerY;
        float centerX;

        if (count > 0)
        {
            // Grouped buttons — use fixed X as before
            centerY = totalY / count;
            centerX = fixedScreenX;
        }
        else
        {
            // Standalone button — position relative to the button
            Vector3[] ownCorners = new Vector3[4];
            container.GetWorldCorners(ownCorners);
            centerY = (ownCorners[0].y + ownCorners[1].y) / 2f;

            float panelWidth = panelRect.rect.width;
            float rightX = ownCorners[2].x + panelWidth / 2f;
            float leftX = ownCorners[0].x - panelWidth / 2f;

            // Check if placing to the right goes off screen
            if (rightX + panelWidth / 2f > Screen.width)
                centerX = leftX;
            else
                centerX = rightX;
        }

        panel.transform.position = new Vector3(centerX, centerY, 0f);
    }

    public void Show(TreeData data, RectTransform container)
    {
        nameText.text = data.treeName;
        statsText.text =
            $"Matures in: {data.minMaturityAgeYears}–{data.maxMaturityAgeYears} years\n" +
            $"Max age: {data.maxAgeYears} years\n" +
            $"Preferred soil: {data.preferredSoil}\n" +
            $"Drought resistance: {(data.droughtResistance * 100f):0}%\n" +
            $"Shade tolerance: {(data.shadeTolerance * 100f):0}%\n" +
            $"Moisture usage: {(data.moistureUsage * 100f):0}%\n" +
            $"Spread chance: {(data.spreadChance * 100f):0}%\n" +
            $"Biodiversity value: {(data.biodiversityValue * 100f):0}%\n";

        Display(container);
    }

    public void Show(string title, string description, RectTransform container)
    {
        nameText.text = title;
        statsText.text = description;
        Display(container);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}