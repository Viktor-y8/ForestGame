using UnityEngine;
using UnityEngine.UI;

public class SkipYearsButton : MonoBehaviour
{

    public void OnClick()
    {
        SoundManager.Instance.PlaySFX("buttonSFX");
        RoundManager.Instance.EndPlanningPhase();
    }
}