using System.Collections.Generic;
using UnityEngine;

public class PestMinigame : MinigameBase
{
    [SerializeField] private RectTransform playArea;
    [SerializeField] private InsectPiece insectPrefab;
    [SerializeField] private float insectSpeed = 80f;

    private List<InsectPiece> activeInsects = new();
    private int totalInsects;

    protected override void OnBegin()
    {
        totalInsects = Context.requiredSuccesses + Random.Range(0, 3);
        activeInsects.Clear();
        for (int i = 0; i < totalInsects; i++) SpawnInsect();
    }

    private void SpawnInsect()
    {
        InsectPiece insect = Instantiate(insectPrefab, playArea);
        insect.GetComponent<RectTransform>().anchoredPosition = new Vector2(
            Random.Range(playArea.rect.xMin, playArea.rect.xMax),
            Random.Range(playArea.rect.yMin, playArea.rect.yMax));
        insect.Initialize(playArea, Random.insideUnitCircle.normalized, insectSpeed);
        insect.OnSquashed += HandleSquashed;
        activeInsects.Add(insect);
    }

    private void HandleSquashed(InsectPiece insect)
    {
        activeInsects.Remove(insect);
        if (activeInsects.Count == 0)
            Complete(new MinigameResult { success = true, completionFraction = 1f });
    }

    public override void ForceEnd() => Complete(BuildTimeoutResult());

    protected override MinigameResult BuildTimeoutResult() =>
        new MinigameResult { success = false, completionFraction = 1f - (float)activeInsects.Count / totalInsects };
}