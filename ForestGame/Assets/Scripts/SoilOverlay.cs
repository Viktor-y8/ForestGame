using UnityEngine;

public class SoilOverlay : MonoBehaviour
{
    [Header("Overlays")]
    [SerializeField] private SpriteRenderer fireOverlay;
    [SerializeField] private SpriteRenderer waterWarningOverlay;

    // Soil moisture below this shows the warning
    [SerializeField] private float lowMoistureThreshold = 0.2f;

    [Header("Bob Settings")]
    [SerializeField] private float bobAmplitude = 0.06f;
    [SerializeField] private float bobSpeed = 2f;

    private Soil soil;
    private Vector3 waterWarningBasePosition;

    [SerializeField] private SpriteRenderer darknessOverlay;  // a black square sprite child

    public void SetDarkness(float amount)
    {
        if (darknessOverlay != null)
            darknessOverlay.color = new Color(0f, 0f, 0f, amount);

        // Also darken the tree sprite so it doesn't bleed over the darkness
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
        waterWarningBasePosition = waterWarningOverlay.transform.localPosition;
    }

    private void Start()
    {
        TimeManager.Instance.OnDayPassed += Refresh;
        Refresh();
    }

    public void Refresh()
    {

        fireOverlay.enabled = soil.isOnFire;

        bool treeIsImmune = soil.CurrentObject is Tree tree && tree.isImmune;

        waterWarningOverlay.enabled = soil.moisture < lowMoistureThreshold && !soil.isOnFire && !treeIsImmune && soil.HasObject;
    }

    private void Update()
    {
        if (!waterWarningOverlay.enabled) return;

        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        waterWarningOverlay.transform.localPosition = waterWarningBasePosition + new Vector3(0f, bob, 0f);
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.OnDayPassed -= Refresh;
    }
}