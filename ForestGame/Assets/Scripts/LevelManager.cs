using System.Collections.Generic;
using UnityEngine;
using static LevelData;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [SerializeField] private WinPanelUI winPanel;

    private LevelData currentLevel;
    private bool hasWon = false;

    public LevelData CurrentLevel => currentLevel;

    private void Awake()
    {
        Instance = this;
    }

    public void SetLevel(LevelData level)
    {
        currentLevel = level;
        hasWon = false;

        TimeManager.Instance.OnDayPassed += CheckWinCondition;
        TimeManager.Instance.OnDayPassed += CheckLossCondition;
    }

    private void CheckWinCondition()
    {
        if (hasWon || currentLevel == null) return;
        if (TutorialManager.IsTutorialActive) return;



        Dictionary<TreeData, int> matureCounts = GetMatureTreeCounts();

        foreach (TreeRequirement req in currentLevel.treeRequirements)
        {
            if (req.requiredMatureCount <= 0) continue;

            int current = matureCounts.ContainsKey(req.treeType) ? matureCounts[req.treeType] : 0;


            if (current < req.requiredMatureCount)
                return;
        }

        TriggerWin();
    }

    private void CheckLossCondition()
    {
        if (hasWon || currentLevel == null)
            return;

        // If the player still has seeds, they can continue.
        if (InteractionManager.Instance.seedCount > 0)
            return;

        // Check if any living tree still exists.
        foreach (Soil soil in GameManager.Instance.GetAllSoils())
        {
            if (soil.isLocked)
                continue;

            if (soil.CurrentObject is Tree tree && !tree.dead)
            {
                // At least one living tree remains.
                return;
            }
        }

        TriggerLoss();
    }

    private void TriggerLoss()
    {

        TimeManager.Instance.scaleTime(0f);

        winPanel.loss = true;

        winPanel.Show();
    }

    public Dictionary<TreeData, int> GetMatureTreeCounts()
    {
        Dictionary<TreeData, int> matureCounts = new();

        foreach (Soil soil in GameManager.Instance.GetAllSoils())
        {
            if (soil.isLocked) continue;

            if (soil.CurrentObject is Tree tree && tree.isMature && !tree.isImmune)
            {
                if (!matureCounts.ContainsKey(tree.data))
                    matureCounts[tree.data] = 0;

                matureCounts[tree.data]++;
            }
        }

        return matureCounts;
    }
    private void TriggerWin()
    {

        hasWon = true;

        winPanel.win = true;

        TimeManager.Instance.scaleTime(0f);
        winPanel.Show();
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnDayPassed -= CheckWinCondition;
            TimeManager.Instance.OnDayPassed -= CheckLossCondition;
        }
    }
}