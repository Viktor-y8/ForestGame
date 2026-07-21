using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    [SerializeField] private string soundName = "buttonSFX";

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(PlaySound);
    }

    private void PlaySound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(soundName);
        }
    }
}