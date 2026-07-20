using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private LevelData levelToLoad;
    [SerializeField] private string gameSceneName = "LevelScene";

    public void LoadLevel()
    {
        LevelSelection.PendingLevel = levelToLoad;
        SceneManager.LoadScene(gameSceneName);
    }
}