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

    private void Refresh()
    {
        fireOverlay.enabled = soil.isOnFire;
        waterWarningOverlay.enabled = soil.moisture < lowMoistureThreshold && !soil.isOnFire;
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