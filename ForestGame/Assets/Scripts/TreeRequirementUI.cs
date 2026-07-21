using TMPro;
using UnityEngine;

public class TreeRequirementUI : MonoBehaviour
{
    [System.Serializable]
    public class RequirementText
    {
        public TreeData tree;
        public TMP_Text text;
    }

    [SerializeField] private RequirementText[] requirements;

    private void Start()
    {
        Refresh();

        TimeManager.Instance.OnDayPassed += Refresh;
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.OnDayPassed -= Refresh;
    }

    public void Refresh()
    {
        if (LevelManager.Instance.CurrentLevel == null)
            return;

        var counts = LevelManager.Instance.GetMatureTreeCounts();

        foreach (var r in requirements)
        {
            int current = counts.TryGetValue(r.tree, out int value)
                ? value
                : 0;

            int required = 0;

            foreach (var req in LevelManager.Instance.CurrentLevel.treeRequirements)
            {
                if (req.treeType == r.tree)
                {
                    required = req.requiredMatureCount;
                    break;
                }
            }

            r.text.text = $"{current}/{required}";
            r.text.color = current >= required ? Color.green : Color.white;
        }
    }
}