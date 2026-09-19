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

        SetVisible();
    }

    private void OnDestroy()
    {
        InteractionManager.OnBudgetChanged -= SetVisible;
        InteractionManager.OnSeedChanged -= SetVisible;
    }

    private void SetVisible()
    {
        if (GameManager.Instance == null ||
            !GameManager.Instance.IsLevelLoaded)
        {
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