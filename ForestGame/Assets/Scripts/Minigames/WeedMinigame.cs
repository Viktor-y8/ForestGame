using System.Collections.Generic;
using UnityEngine;

public class WeedMinigame : MinigameBase
{
    [SerializeField] private RectTransform spawnArea;
    [SerializeField] private WeedPiece weedPiecePrefab;

    private List<WeedPiece> activePieces = new();
    private int totalWeeds;

    protected override void OnBegin()
    {
        spawnArea.SetAsLastSibling();
        totalWeeds = Context.requiredSuccesses;
        totalWeeds += Random.Range(-2, 2);

        activePieces.Clear();
        for (int i = 0; i < totalWeeds; i++) SpawnWeed();
    }

    private void SpawnWeed()
    {
        WeedPiece piece = Instantiate(weedPiecePrefab, spawnArea);
        piece.GetComponent<RectTransform>().anchoredPosition = new Vector2(
            Random.Range(spawnArea.rect.xMin +  60, spawnArea.rect.xMax - 60),
            spawnArea.rect.yMin);
        piece.OnClicked += HandleWeedClicked;
        activePieces.Add(piece);
    }

    private void HandleWeedClicked(WeedPiece piece)
    {
        activePieces.Remove(piece);
        piece.PlayClearedAnim(() => Destroy(piece.gameObject));
        if (activePieces.Count == 0)
            Complete(new MinigameResult { success = true, completionFraction = 1f });
    }

    public override void ForceEnd() => Complete(BuildTimeoutResult());

    protected override MinigameResult BuildTimeoutResult() =>
        new MinigameResult
        {
            success = activePieces.Count == 0,
            completionFraction = 1f - (float)activePieces.Count / totalWeeds
        };
}