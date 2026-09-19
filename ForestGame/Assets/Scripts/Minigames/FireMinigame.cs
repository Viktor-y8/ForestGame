using UnityEngine;
using UnityEngine.UI;

public class FireMinigame : MinigameBase
{
    [SerializeField] private RectTransform fireArea;
    [SerializeField] private RectTransform ring;
    [SerializeField] private Image fillIndicator;
    [SerializeField] private float ringHitRadius = 50f;
    [SerializeField] private float fillRatePerSecond = 0.6f;
    [SerializeField] private float drainRatePerSecond = 0.5f;

    private int successesNeeded, successesSoFar;
    private float currentFill;

    protected override void OnBegin()
    {
        successesNeeded = Context.requiredSuccesses;
        successesSoFar = 0;
        SpawnRing();
    }

    private void Update()
    {
        if (finished) return;

        bool holding = Input.GetMouseButton(0) && PointerNearRing();
        currentFill = Mathf.Clamp01(currentFill - (holding ? fillRatePerSecond : -drainRatePerSecond) * Time.deltaTime);
        fillIndicator.fillAmount = currentFill;

        if (currentFill <= 0f)
        {
            SoundManager.Instance.PlaySFX("waterSFX");
            successesSoFar++;
            if (successesSoFar >= successesNeeded)
                Complete(new MinigameResult { success = true, completionFraction = 1f });
            else
                SpawnRing();
        }
    }

    private bool PointerNearRing()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(fireArea, Input.mousePosition, null, out Vector2 local);
        return Vector2.Distance(local, ring.anchoredPosition) <= ringHitRadius;
    }

    private void SpawnRing()
    {
        currentFill = 1f;
        fillIndicator.fillAmount = 0f;
        ring.anchoredPosition = new Vector2(
            Random.Range(fireArea.rect.xMin + ringHitRadius, fireArea.rect.xMax - ringHitRadius),
            Random.Range(fireArea.rect.yMin + ringHitRadius, fireArea.rect.yMax - ringHitRadius));
    }

    public override void ForceEnd() => Complete(BuildTimeoutResult());

    protected override MinigameResult BuildTimeoutResult() =>
        new MinigameResult { success = false, completionFraction = (float)successesSoFar / successesNeeded };
}