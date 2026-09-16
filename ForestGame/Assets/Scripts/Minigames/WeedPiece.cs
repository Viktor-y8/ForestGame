using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class WeedPiece : MonoBehaviour, IPointerClickHandler
{
    public event Action<WeedPiece> OnClicked;

    [Header("Clear animation")]
    [SerializeField] private float clearDuration = 0.15f;
    [SerializeField]
    private AnimationCurve clearScaleCurve =
        AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("Optional feedback")]
    [SerializeField] private ParticleSystem clearedParticles;
    [SerializeField] private AudioClip clearedSfx;

    private bool cleared;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (cleared) return;
        cleared = true;

        OnClicked?.Invoke(this);
    }

    public void PlayClearedAnim(Action onComplete)
    {
        if (clearedParticles != null)
            Instantiate(clearedParticles, rectTransform.position, Quaternion.identity);

        if (clearedSfx != null)
            SoundManager.Instance.PlaySFX(clearedSfx.name, 1f, 0.05f);

        StartCoroutine(ClearAnimRoutine(onComplete));
    }

    private IEnumerator ClearAnimRoutine(Action onComplete)
    {
        float t = 0f;
        Vector3 startScale = rectTransform.localScale;

        while (t < clearDuration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / clearDuration);
            float scale = clearScaleCurve.Evaluate(normalized);
            rectTransform.localScale = startScale * scale;
            yield return null;
        }

        rectTransform.localScale = Vector3.zero;
        onComplete?.Invoke();
    }
}