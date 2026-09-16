using System;
using System.Collections;
using UnityEngine;

public abstract class MinigameBase : MonoBehaviour
{
    public event Action<MinigameResult> OnFinished;
    public event Action<float, float> OnTimeUpdated; // (remaining, total)

    protected MinigameContext Context { get; private set; }
    protected bool finished;

    public void Setup(MinigameContext context) => Context = context;

    public void Begin()
    {
        finished = false;
        OnBegin();
        StartCoroutine(TimerRoutine());
    }

    protected abstract void OnBegin();
    public abstract void ForceEnd();
    protected abstract MinigameResult BuildTimeoutResult();

    private IEnumerator TimerRoutine()
    {
        if (Context.timeLimit <= 0f) yield break;

        float remaining = Context.timeLimit;
        while (remaining > 0f && !finished)
        {
            remaining -= Time.deltaTime;
            OnTimeUpdated?.Invoke(Mathf.Max(0f, remaining), Context.timeLimit);
            yield return null;
        }
        if (!finished) Complete(BuildTimeoutResult());
    }

    protected void Complete(MinigameResult result)
    {
        if (finished) return;
        finished = true;
        OnFinished?.Invoke(result);
    }
}