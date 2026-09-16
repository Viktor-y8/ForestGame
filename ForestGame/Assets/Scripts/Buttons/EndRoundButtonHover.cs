using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EndRoundButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private HashSet<Soil> currentPreviewSet;

    private void OnEnable()
    {
        RoundManager.Instance.OnPhaseChanged += HandlePhaseChanged;
    }

    private void OnDisable()
    {
        if (RoundManager.Instance != null)
            RoundManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (RoundManager.Instance.Phase != GamePhase.Planning) return;
        SetAllForecastsVisible(true);
        ShowFirePreview(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetAllForecastsVisible(false);
        ShowFirePreview(false);
    }

    private void HandlePhaseChanged(GamePhase phase)
    {
        if (phase != GamePhase.Planning)
        {
            SetAllForecastsVisible(false);
            ShowFirePreview(false);
        }
    }


    private void SetAllForecastsVisible(bool visible)
    {
        foreach (Soil soil in GameManager.Instance.GetAllSoils())
        {
            if (soil.isLocked) continue;
            if (soil.CurrentObject is not Tree tree) continue;
            if (soil.isOnFire) continue;

            TreeForecastLabel forecast = tree.GetComponent<TreeForecastLabel>();
            if (forecast == null) continue;

            if (visible)
                forecast.Show(tree.health * 100f, tree.PredictedHealthDeltaPercent(), tree.WillDieNextRound());
            else
                forecast.Hide();
        }
    }

    private void ShowFirePreview(bool visible)
    {
        if (visible)
        {
            currentPreviewSet = RoundManager.Instance.PredictBurnSpread();
            foreach (Soil s in currentPreviewSet)
                s.GetComponent<SoilOverlay>()?.ShowFirePreview();
        }
        else if (currentPreviewSet != null)
        {
            foreach (Soil s in currentPreviewSet)
                s.GetComponent<SoilOverlay>()?.HideFirePreview();
            currentPreviewSet = null;
        }
    }
}