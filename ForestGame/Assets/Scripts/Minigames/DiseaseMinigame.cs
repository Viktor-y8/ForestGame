using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DiseaseMinigame : MinigameBase
{
    [SerializeField] private Image treeImage;

    [SerializeField] private Sprite[] normalSprites;
    [SerializeField] private Sprite[] dieseasedSprites;
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeStrength = 10f;

    private int hitsNeeded, hitsSoFar;
    private RectTransform treeRect;
    private Vector2 basePos;

    protected override void OnBegin()
    {
        hitsNeeded = 4;
        hitsSoFar = 0;
        treeRect = treeImage.rectTransform;
        basePos = treeRect.anchoredPosition;

        if (Context.targetTree != null && Context.targetTree.hasDisease) treeImage.sprite = dieseasedSprites[0];
        else if (Context.targetTree != null && !Context.targetTree.hasDisease) treeImage.sprite = normalSprites[0];
    }

    // Hooked up as this component's OnClick via an EventTrigger/IPointerClickHandler on treeImage
    public void OnTreeClicked()
    {
        if (finished) return;

        SoundManager.Instance.PlaySFX("buttonSFX");

        hitsSoFar++;
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine());

        Sprite[] sprites = (Context.targetTree != null && Context.targetTree.hasDisease) ? dieseasedSprites : normalSprites;

        /*switch(hitsSoFar)
        {
            case 1: treeImage.sprite = choppedSprite; break;
            case 2: treeImage.sprite = choppedSprite2; break;
            case 3: treeImage.sprite = choppedSprite3; break;
            case 4: treeImage.sprite = choppedSprite3; break;
        }*/

        treeImage.sprite = sprites[hitsSoFar];

        if (hitsSoFar >= hitsNeeded)
            Complete(new MinigameResult { success = true, completionFraction = 1f });
    }

    private IEnumerator ShakeRoutine()
    {
        float t = 0f;
        while (t < shakeDuration)
        {
            t += Time.deltaTime;
            float offset = Mathf.Sin(t * 50f) * shakeStrength * (1f - t / shakeDuration);
            treeRect.anchoredPosition = basePos + new Vector2(offset, 0f);
            yield return null;
        }
        treeRect.anchoredPosition = basePos;
    }

    public override void ForceEnd() => Complete(BuildTimeoutResult());

    protected override MinigameResult BuildTimeoutResult() =>
        new MinigameResult { success = false, completionFraction = (float)hitsSoFar / hitsNeeded };
}