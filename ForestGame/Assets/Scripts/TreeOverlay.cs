using UnityEngine;

public class TreeOverlay : MonoBehaviour
{
    [SerializeField] private SpriteRenderer skullOverlay;
    [SerializeField] private float lowHealthThreshold = 0.25f;

    [Header("Bob Settings")]
    [SerializeField] private float bobAmplitude = 0.06f;
    [SerializeField] private float bobSpeed = 2f;

    private Tree tree;
    private Vector3 skullBasePosition;
    private bool basePositionSet = false;

    private void Awake()
    {
        tree = GetComponent<Tree>();
    }

    private void Start()
    {
        TimeManager.Instance.OnDayPassed += Refresh;
        Refresh();
    }

    // Call this whenever the tree sprite changes stage
    public void RefreshSkullPosition()
    {
        SpriteRenderer treeSprite = GetComponent<SpriteRenderer>();
        if (treeSprite == null || treeSprite.sprite == null) return;

        Bounds bounds = treeSprite.sprite.bounds;

        // Top-right corner of whatever sprite is currently shown
        skullBasePosition = new Vector3(bounds.max.x, bounds.max.y, 0f);
        skullOverlay.transform.localPosition = skullBasePosition;
        basePositionSet = true;
    }

    private void Refresh()
    {
        skullOverlay.enabled = !tree.dead && tree.health < lowHealthThreshold && !tree.isImmune;

        if (!basePositionSet)
            RefreshSkullPosition();
    }

    private void Update()
    {
        if (!skullOverlay.enabled) return;

        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        skullOverlay.transform.localPosition = skullBasePosition + new Vector3(0f, bob, 0f);
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.OnDayPassed -= Refresh;
    }
}