using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private LevelData levelToLoad;
    [SerializeField] private string gameSceneName = "LevelScene";

    public void LoadLevel()
    {
        SoundManager.Instance.PlaySFX("buttonSFX");
        LevelSelection.PendingLevel = levelToLoad;
        SceneManager.LoadScene(gameSceneName);
    }
}