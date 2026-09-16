using TMPro;
using UnityEngine;

public class RoundSummaryUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text statsText;

    private void Awake()
    {
        panelRoot.SetActive(false);
        RoundManager.Instance.OnPhaseChanged += HandlePhaseChanged;
    }

    private void HandlePhaseChanged(GamePhase phase)
    {
        if (phase == GamePhase.RoundSummary)
            Show();
        else
            panelRoot.SetActive(false);
    }

    private void Show()
    {
        panelRoot.SetActive(true);

        int round = RoundManager.Instance.CurrentRound;
        Season season = RoundManager.Instance.CurrentSeason;

        titleText.text = $"Round {round} complete";

        statsText.text =
            $"Season ahead: {season}\n" +
            $"Trees lost: {RoundManager.Instance.treesLostThisRound}\n" +
            $"Trees standing: {RoundManager.Instance.treesSavedThisRound}";
    }

    public void OnContinuePressed()
    {
        SoundManager.Instance.PlaySFX("buttonSFX");
        RoundManager.Instance.AcknowledgeRoundSummary();
    }

    private void OnDestroy()
    {
        if (RoundManager.Instance != null)
            RoundManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
    }
}