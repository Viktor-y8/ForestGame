using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private string menuSceneName = "MainMenu";

    private void Awake()
    {
        panelRoot.SetActive(false);
    }

    public void Show()
    {
        panelRoot.SetActive(true);
        messageText.text = "You reforested the land!";
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f; // safety reset in case anything reads Unity's own timeScale
        SceneManager.LoadScene(menuSceneName);
    }
}