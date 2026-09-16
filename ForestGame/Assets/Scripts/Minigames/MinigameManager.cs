using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance;

    [SerializeField] private Canvas overlayCanvas;
    [SerializeField] private GameObject dimBackground;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text hintText;

    [System.Serializable]
    public struct MinigameCursorEntry
    {
        public MinigameType type;
        public Texture2D cursorTexture;
        public Vector2 hotspot;
    }
    [SerializeField] private List<MinigameCursorEntry> minigameCursors;
    private Dictionary<MinigameType, MinigameCursorEntry> cursorLookup;

    [System.Serializable]
    public struct MinigamePrefabEntry
    {
        public MinigameType type;
        public MinigameBase prefab;
    }
    [SerializeField] private List<MinigamePrefabEntry> registeredMinigames;
    private Dictionary<MinigameType, MinigameBase> prefabLookup;

    private MinigameBase activeMinigame;
    private Action<MinigameResult> pendingCallback;
    private Queue<(MinigameContext ctx, Action<MinigameResult> cb)> queue = new();

    private void Awake()
    {
        Instance = this;
        prefabLookup = registeredMinigames.ToDictionary(e => e.type, e => e.prefab);
        cursorLookup = minigameCursors.ToDictionary(e => e.type, e => e);
        overlayCanvas.gameObject.SetActive(false);
    }

    public bool IsMinigameActive => activeMinigame != null;

    public void Open(MinigameContext context, Action<MinigameResult> onComplete)
    {
        if (IsMinigameActive)
        {
            queue.Enqueue((context, onComplete));
            return;
        }

        StartMinigame(context, onComplete);
    }

    private void StartMinigame(MinigameContext context, Action<MinigameResult> onComplete)
    {
        InteractionManager.Instance.HideToolPreview();

        pendingCallback = onComplete;
        overlayCanvas.gameObject.SetActive(true);
        dimBackground.SetActive(true);
        hintText.text = context.hintText;
        timerText.text = context.timeLimit > 0f ? Mathf.CeilToInt(context.timeLimit).ToString() : "";

        activeMinigame = Instantiate(prefabLookup[context.type], overlayCanvas.transform);
        activeMinigame.Setup(context);
        activeMinigame.OnFinished += HandleFinished;
        activeMinigame.OnTimeUpdated += HandleTimeUpdated;

        timerText.transform.SetAsLastSibling();
        hintText.transform.SetAsLastSibling();

        if (cursorLookup.TryGetValue(context.type, out var cursor))
            GameCursor.Set(cursor.cursorTexture, cursor.hotspot);

        InteractionManager.Instance.SetInputLocked(true);
        activeMinigame.Begin();
    }

    private void HandleTimeUpdated(float remaining, float total) =>
        timerText.text = total > 0f ? Mathf.CeilToInt(remaining).ToString() : "";

    private void HandleFinished(MinigameResult result)
    {
        activeMinigame.OnFinished -= HandleFinished;
        activeMinigame.OnTimeUpdated -= HandleTimeUpdated;
        Destroy(activeMinigame.gameObject);
        activeMinigame = null;

        overlayCanvas.gameObject.SetActive(false);
        dimBackground.SetActive(false);
        InteractionManager.Instance.SetInputLocked(false);
        InteractionManager.Instance.ApplyCursorForCurrentTool();

        pendingCallback?.Invoke(result);
        pendingCallback = null;

        if (queue.Count > 0)
        {
            var (ctx, cb) = queue.Dequeue();
            StartMinigame(ctx, cb);
        }
    }
}
