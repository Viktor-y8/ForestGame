using System.Collections.Generic;
using UnityEngine;
using static LevelData;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [SerializeField] private WinPanelUI winPanel;

    private LevelData currentLevel;
    private bool hasWon = false;

    private void Awake()
    {
        Instance = this;
    }

    public void SetLevel(LevelData level)
    {
        currentLevel = level;
        hasWon = false;

        TimeManager.Instance.OnDayPassed += CheckWinCondition;
    }

    private void CheckWinCondition()
    {
        if (hasWon || currentLevel == null) return;
        if (TutorialManager.IsTutorialActive) return;



        Dictionary<TreeData, int> matureCounts = new Dictionary<TreeData, int>();

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

        foreach (TreeRequirement req in currentLevel.treeRequirements)
        {
            if (req.requiredMatureCount <= 0) continue;

            int current = matureCounts.ContainsKey(req.treeType) ? matureCounts[req.treeType] : 0;


            if (current < req.requiredMatureCount)
                return;
        }

        TriggerWin();
    }

    private void TriggerWin()
    {
        hasWon = true;
        TimeManager.Instance.scaleTime(0f);
        winPanel.Show();
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.OnMonthPassed -= CheckWinCondition;
    }
}