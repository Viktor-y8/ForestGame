using UnityEngine;

public class SoilOverlay : MonoBehaviour
{
    [Header("Overlays")]
    [SerializeField] private SpriteRenderer fireOverlay;
    [SerializeField] private SpriteRenderer waterOverlay;
    [SerializeField] private SpriteRenderer fertilizedOverlay;
    [SerializeField] private Sprite fertilizedMid;
    [SerializeField] private Sprite fertilizedMax;

    [SerializeField] private float lowMoistureThreshold = 0.2f;

    [Header("Bob Settings")]
    [SerializeField] private float bobAmplitude = 0.06f;
    [SerializeField] private float bobSpeed = 2f;

    private Soil soil;
    private Vector3 waterBasePosition;
    private Vector3 fertilizedBasePosition;

    [SerializeField] private SpriteRenderer darknessOverlay;

    private const int overlaySortBonus = 20;
    private const int immuneTreeOverrideOrder = 10000;

    public void ShowFirePreview()
    {
        if (soil.isOnFire) return;
        fireOverlay.enabled = true;
        fireOverlay.color = new Color(1f, 1f, 1f, 0.4f); 
    }

    public void HideFirePreview()
    {
        if (soil.isOnFire) return;
        fireOverlay.enabled = false;
        fireOverlay.color = Color.white; 
    }

    public void SetDarkness(float amount)
    {
        if (darknessOverlay != null)
            darknessOverlay.color = new Color(0f, 0f, 0f, amount);

        if (soil.CurrentObject != null)
        {
            SpriteRenderer treeSr = soil.CurrentObject.GetComponent<SpriteRenderer>();
            if (treeSr != null)
            {
                float brightness = 1f - amount;
                treeSr.color = new Color(brightness, brightness, brightness, 1f);
            }
        }
    }

    private void Awake()
    {
        soil = GetComponent<Soil>();
        waterBasePosition = waterOverlay.transform.localPosition;
        fertilizedBasePosition = fertilizedOverlay.transform.localPosition;
    }

    private void Start()
    {
        soil.OnStateChanged += Refresh;
        Refresh();
    }

    public void Refresh()
    {
        fireOverlay.enabled = soil.isOnFire;
        fireOverlay.color = Color.white;

        bool treeIsImmune = soil.CurrentObject is Tree tree && tree.isImmune;

        waterOverlay.enabled = soil.isWatered && !treeIsImmune && soil.CurrentObject is Tree;
        fertilizedOverlay.enabled = soil.fertilized > 0 && !treeIsImmune && soil.CurrentObject is Tree;

        if (soil.fertilized == 1)
            fertilizedOverlay.sprite = fertilizedMid;
        else if (soil.fertilized == 2)
            fertilizedOverlay.sprite = fertilizedMax;
    }

    private void Update()
    {
        if (!waterOverlay.enabled && !fertilizedOverlay.enabled) return;

        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        waterOverlay.transform.localPosition = waterBasePosition + new Vector3(0f, bob, 0f);
        fertilizedOverlay.transform.localPosition = fertilizedBasePosition + new Vector3(0f, bob, 0f);
    }

    void LateUpdate()
    {
        bool immuneTreeNearby = HasImmuneTreeInFront();

        int order = immuneTreeNearby
            ? immuneTreeOverrideOrder
            : Mathf.RoundToInt(-transform.position.y * 100) + overlaySortBonus;

        waterOverlay.sortingOrder = order;
        fertilizedOverlay.sortingOrder = order;
    }

    private bool HasImmuneTreeInFront()
    {
        if (soil.CurrentObject is Tree ownTree && ownTree.isImmune) return true;

        Soil inFront = soil.grid.GetValue(soil.x, soil.y - 1);
        if (inFront != null && inFront.CurrentObject is Tree frontTree && frontTree.isImmune)
            return true;

        return false;
    }

    private void OnDestroy()
    {
        if (soil != null) soil.OnStateChanged -= Refresh;
    }
}