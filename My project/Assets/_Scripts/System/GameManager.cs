using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Loading scenes")]
    public GameObject loadingCanva;

    [Header("UI elements")]
    public Slider progressBar;

    private string _sceneToLoad;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        ResumeGame();
    }

    private void Start()
    {
        if(progressBar != null)
        {
            progressBar.interactable = false;
        }

        if(loadingCanva != null)
        {
            loadingCanva.SetActive(false);
        }
    }

    public void StartLoadingScene(string sceneName)
    {
        _sceneToLoad = sceneName;

        if(loadingCanva != null)
        {
            loadingCanva.SetActive(true);
        }
        else
        {
            Debug.LogWarning("No loading canva detected!");
        }

        StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(0.1f);
        
        loadingCanva?.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(_sceneToLoad);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            if(progressBar != null)
            {
                progressBar.value = operation.progress;
            }

            yield return null;

            if(operation.progress >= 0.9f)
            {
                operation.allowSceneActivation = true;
            }
        }

        loadingCanva?.SetActive(false);
    }

    public void HideMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        ShowMouse();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        HideMouse();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        ResumeGame();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
