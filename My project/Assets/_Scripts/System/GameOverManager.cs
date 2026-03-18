using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public void RestartGame()
    {
        GameManager.Instance.StartLoadingScene("Main_Maze");
    }

    public void ReturnToMainMenu()
    {
        GameManager.Instance.StartLoadingScene("MenuScene");
    }

    public void QuitGame()
    {
        GameManager.Instance.QuitGame();
    }
}
