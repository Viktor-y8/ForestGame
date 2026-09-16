using UnityEngine;

public class TreeOverlay : MonoBehaviour
{
    [SerializeField] private SpriteRenderer eventIcon;
    [SerializeField] private Sprite pestSprite;
    [SerializeField] private Sprite diseaseSprite;

    [Header("Bob Settings")]
    [SerializeField] private float bobAmplitude = 0.06f;
    [SerializeField] private float bobSpeed = 2f;

    private Tree tree;
    private Vector3 basePosition;
    private bool basePositionSet;

    [SerializeField] private float heightOffset = 0.9f;

    private void Awake() => tree = GetComponentInParent<Tree>();
    private void Start() => Refresh();

    public void RefreshPosition()
    {
        basePosition = new Vector3(0f, heightOffset, 0f);
        eventIcon.transform.localPosition = basePosition;
        basePositionSet = true;
    }

    public void Refresh()
    {
        if (!basePositionSet) RefreshPosition();

        if (tree.dead) { eventIcon.enabled = false; return; }

        if (tree.hasDisease) { eventIcon.enabled = true; eventIcon.sprite = diseaseSprite; }
        else if (tree.hasPest) { eventIcon.enabled = true; eventIcon.sprite = pestSprite; }
        else { eventIcon.enabled = false; }
    }

    private void Update()
    {
        if (!eventIcon.enabled) return;
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        eventIcon.transform.localPosition = basePosition + new Vector3(0f, bob, 0f);
    }

    private void LateUpdate()
    {
        eventIcon.sortingOrder = Mathf.RoundToInt(-tree.transform.position.y * 100) + 30;
    }
}