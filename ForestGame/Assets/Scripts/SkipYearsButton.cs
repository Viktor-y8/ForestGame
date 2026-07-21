using UnityEngine;
using UnityEngine.UI;

public class SkipYearsButton : MonoBehaviour
{
    [SerializeField] private int yearsToSkip = 10;
    [SerializeField] private GameObject loadingIndicator; // simple "Simulating..." panel

    public void Awake()
    {
        loadingIndicator.SetActive(false);
    }

    public void OnClick()
    {
        SoundManager.Instance.PlaySFX("buttonSFX");
        loadingIndicator.SetActive(true);
        TimeManager.Instance.SkipYears(yearsToSkip);
        StartCoroutine(HideLoadingWhenDone());
    }

    private System.Collections.IEnumerator HideLoadingWhenDone()
    {
        yield return new WaitUntil(() => !TimeManager.IsFastForwarding);
        loadingIndicator.SetActive(false);
    }
}