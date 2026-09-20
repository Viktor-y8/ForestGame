using UnityEngine;

public class UseAllButton : MonoBehaviour
{
    [SerializeField] private bool isWater;

    [Header("First Container")]
    [SerializeField] private GameObject firstContainerButton;

    private void Start()
    {
        InteractionManager.OnBudgetChanged += SetVisible;
        InteractionManager.OnSeedChanged += SetVisible;
        RoundManager.Instance.OnPhaseSettled += HandlePhaseSettled;
        Tree.OnAnyTreeChanged += SetVisible;

        SetVisible();
    }

    private void OnEnable()
    {
        SetVisible();
    }

    private void OnDestroy()
    {
        InteractionManager.OnBudgetChanged -= SetVisible;
        InteractionManager.OnSeedChanged -= SetVisible;
        if (RoundManager.Instance != null)
            RoundManager.Instance.OnPhaseSettled -= HandlePhaseSettled;
        Tree.OnAnyTreeChanged -= SetVisible;
    }

    private void HandlePhaseSettled(GamePhase phase) => SetVisible();

    private void SetVisible()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsLevelLoaded)
        {
            gameObject.SetActive(false);
            return;
        }

        if (firstContainerButton == null || !firstContainerButton.activeSelf)
        {
            gameObject.SetActive(false);
            return;
        }

        if (isWater)
            gameObject.SetActive(InteractionManager.Instance.CanWaterAllTrees());
        else
            gameObject.SetActive(InteractionManager.Instance.CanFertilizeAllTrees());
    }
}