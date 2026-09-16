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
    }

    public bool CheckEndConditions()
    {
        if (currentLevel == null || hasWon) return false;

        if (HasMetWinRequirements())
        {
            TriggerWin();
            return true;
        }

        if (HasLost())
        {
            TriggerLoss();
            return true;
        }

        return false;
    }

    private bool HasMetWinRequirements()
    {
        if (TutorialManager.IsTutorialActive) return false;

        Dictionary<TreeData, int> matureCounts = GetMatureTreeCounts();

        foreach (TreeRequirement req in currentLevel.treeRequirements)
        {
            if (req.requiredMatureCount <= 0) continue;
            int current = matureCounts.TryGetValue(req.treeType, out int c) ? c : 0;
            if (current < req.requiredMatureCount) return false;
        }

        return true;
    }

    private bool HasLost()
    {
        if (InteractionManager.Instance.seedCount > 0) return false;

        foreach (Soil soil in GameManager.Instance.GetAllSoils())
        {
            if (soil.isLocked) continue;
            if (soil.CurrentObject is Tree tree && !tree.dead) return false;
        }

        return true;
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

        if (InteractionManager.Instance.seedCount > 0)
            return;

        foreach (Soil soil in GameManager.Instance.GetAllSoils())
        {
            if (soil.isLocked)
                continue;

            if (soil.CurrentObject is Tree tree && !tree.dead)
            {
                return;
            }
        }

        TriggerLoss();
    }

    public void CheckLossImmediate()
    {
        if (currentLevel == null || hasWon) return;
        if (RoundManager.Instance.Phase != GamePhase.Planning) return;

        if (HasLost())
        {
            TriggerLoss();
        }
    }

    private void TriggerLoss()
    {

        winPanel.loss = true;
        winPanel.win = false;

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
        winPanel.loss = false;

        winPanel.Show();
    }

}