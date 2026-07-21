using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private string menuSceneName = "MainMenu";

    public bool win = false;
    public bool loss = false;

    public bool playSound = false;

    private void Awake()
    {
        panelRoot.SetActive(false);
    }

    public void Show()
    {
        panelRoot.SetActive(true);

        int treesPlanted = InteractionManager.Instance.treesPlanted;
        int treesDied = InteractionManager.Instance.treesDied;
        int firesStarted = InteractionManager.Instance.firesStarted;
        int waterToolsUsed = InteractionManager.Instance.waterToolsUsed;
        int ditchToolsUsed = InteractionManager.Instance.ditchToolsUsed;

        if (win)
        {
            messageText.text = "You reforested the land!";
            if (!playSound)
            {
                SoundManager.Instance.PlaySFX("winSFX");
                playSound = true;
            }
        }
        else if(loss)
        {
            messageText.text = "You failed to reforest the land...";
            if (!playSound)
            {
                SoundManager.Instance.PlaySFX("loseSFX");
                playSound = true;
            }
        }

        infoText.text = $"Trees Planted: {treesPlanted}\r\nTrees Died: {treesDied}\r\nFires Started: {firesStarted}\r\nWater tool used: {waterToolsUsed}\r\nDitches used: {ditchToolsUsed}";
    }

    public void GoToMenu()
    {
        SoundManager.Instance.PlaySFX("buttonSFX");

        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);

        SoundManager.Instance.StopAllLoopingSFX();

        SoundManager.Instance.PlayMusic(SoundManager.Instance.menuMusic);
    }
}