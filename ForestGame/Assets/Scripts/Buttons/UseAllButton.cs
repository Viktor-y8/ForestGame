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

    private void OnEnable()
    {
        SetVisible();
    }

    private void OnDestroy()
    {
        InteractionManager.OnBudgetChanged -= SetVisible;
        InteractionManager.OnSeedChanged -= SetVisible;
    }

    private void SetVisible()
    {
        // Level hasn't finished loading
        if (GameManager.Instance == null || !GameManager.Instance.IsLevelLoaded)
        {
            gameObject.SetActive(false);
            return;
        }

        // Second container is currently being shown
        if (firstContainerButton == null || !firstContainerButton.activeSelf)
        {
            gameObject.SetActive(false);
            return;
        }

        // Check if Use All can actually be used
        if (isWater)
            gameObject.SetActive(InteractionManager.Instance.CanWaterAllTrees());
        else
            gameObject.SetActive(InteractionManager.Instance.CanFertilizeAllTrees());
    }
}