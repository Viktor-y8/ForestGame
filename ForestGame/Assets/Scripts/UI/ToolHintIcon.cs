using UnityEngine;

public class ToolHintIcon : MonoBehaviour
{
    [SerializeField] private SpriteRenderer iconRenderer;
    [SerializeField] private float bobAmplitude = 0.05f;
    [SerializeField] private float bobSpeed = 3f;
    [SerializeField, Range(0f, 1f)] private float alpha = 0.6f;

    private Vector3 basePosition;
    private bool basePositionSet;

    private void Awake() => iconRenderer.enabled = false;

    public void RefreshPosition()
    {
        SpriteRenderer treeSprite = GetComponent<SpriteRenderer>();
        if (treeSprite == null || treeSprite.sprite == null) return;

        Bounds bounds = treeSprite.sprite.bounds;
        basePosition = new Vector3(bounds.center.x, bounds.max.y + 0.3f, 0f);
        iconRenderer.transform.localPosition = basePosition;
        basePositionSet = true;
    }

    public void Show(ToolType tool)
    {
        Sprite sprite = InteractionManager.Instance.GetToolSprite(tool);
        if (sprite == null) return;

        if (!basePositionSet) RefreshPosition();

        iconRenderer.sprite = sprite;
        iconRenderer.color = new Color(1f, 1f, 1f, alpha);
        iconRenderer.enabled = true;
    }

    public void Hide() => iconRenderer.enabled = false;

    private void Update()
    {
        if (!iconRenderer.enabled) return;
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        iconRenderer.transform.localPosition = basePosition + new Vector3(0f, bob, 0f);
    }
}