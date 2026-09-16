using TMPro;
using UnityEngine;
public class SeedCounter : MonoBehaviour
{

    [SerializeField] private TMP_Text text;
    private void Start()
    {
        RefreshCountText();
        InteractionManager.OnSeedChanged += RefreshCountText;
    }

    private void OnDestroy()
    {
        InteractionManager.OnSeedChanged -= RefreshCountText;
    }

    private void RefreshCountText()
    {
        if (text == null) return;

        text.text = InteractionManager.Instance.seedCount.ToString();
    }


}
