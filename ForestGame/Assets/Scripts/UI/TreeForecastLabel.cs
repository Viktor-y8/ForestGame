using TMPro;
using UnityEngine;

public class TreeForecastLabel : MonoBehaviour
{
    [SerializeField] private TextMeshPro label;
    [SerializeField] private SpriteRenderer skullIcon;
    [SerializeField] private Color positiveColor = new Color(0.4f, 0.9f, 0.4f);
    [SerializeField] private Color negativeColor = new Color(0.9f, 0.3f, 0.3f);

    private MeshRenderer labelRenderer;
    private Tree tree;
    private Vector3 basePosition;
    private bool basePositionSet;

    [SerializeField] private float heightOffset = 1.3f;

    private void Awake()
    {
        labelRenderer = label.GetComponent<MeshRenderer>();
        labelRenderer.sortingLayerName = "Objects";
        skullIcon.sortingLayerName = "Objects";
        label.alignment = TextAlignmentOptions.Center;

        tree = GetComponentInParent<Tree>();

        labelRenderer.enabled = false;
        skullIcon.enabled = false;
    }

    public void RefreshPosition()
    {
        basePosition = new Vector3(0f, heightOffset, 0f); // local to tree root — X=0 keeps it centered
        label.transform.localPosition = basePosition;
        skullIcon.transform.localPosition = basePosition;
        basePositionSet = true;
    }

    public void Show(float currentHealth, float deltaPercent, bool willDie, bool endRound)
    {
        if (!basePositionSet) RefreshPosition();

        if (willDie)
        {
            skullIcon.enabled = true;
            labelRenderer.enabled = false;
        }
        else
        {
            skullIcon.enabled = false;
            labelRenderer.enabled = true;
            if (endRound)
            {
                string sign = deltaPercent >= 0 ? "\n+" : "";
                label.text = $"{currentHealth:0}% {sign}{deltaPercent:0}%";
                label.color = deltaPercent >= 0 ? positiveColor : negativeColor;
            }
            else
            {
                label.text = $"{currentHealth:0}%";
                label.color = Color.white;
            }
        }
    }

    public void Hide()
    {
        labelRenderer.enabled = false;
        skullIcon.enabled = false;
    }

    private void LateUpdate()
    {
        int order = Mathf.RoundToInt(-tree.transform.position.y * 100) + 50;
        labelRenderer.sortingOrder = order;
        skullIcon.sortingOrder = order;
    }
}