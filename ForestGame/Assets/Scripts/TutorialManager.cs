using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [SerializeField] private TutorialPanelUI panelUI;

    private HashSet<string> shownSteps = new HashSet<string>();
    private Queue<TutorialStep> queue = new Queue<TutorialStep>();
    private bool isShowing = false;

    public static bool IsTutorialActive { get; private set; } = false;

    private void Awake()
    {
        Instance = this;
    }

    // Call this anywhere — e.g. when player plants their first tree,
    // when a fire starts for the first time, etc.
    public void TriggerTutorial(TutorialStep step)
    {
        if (step == null) return;
        if (shownSteps.Contains(step.stepId)) return; // already seen

        shownSteps.Add(step.stepId);
        queue.Enqueue(step);

        if (!isShowing)
            ShowNext();
    }

    // For manually triggering, e.g. from a "Help" button, ignoring the once-only rule
    public void ForceShow(TutorialStep step)
    {
        queue.Enqueue(step);
        if (!isShowing)
            ShowNext();
    }

    private void ShowNext()
    {
        if (queue.Count == 0)
        {
            isShowing = false;
            IsTutorialActive = false;
            return;
        }

        isShowing = true;
        IsTutorialActive = true;
        TutorialStep step = queue.Dequeue();
        panelUI.Show(step, OnStepClosed);
    }

    private void OnStepClosed()
    {
        ShowNext(); // show the next queued step, if any
    }
}