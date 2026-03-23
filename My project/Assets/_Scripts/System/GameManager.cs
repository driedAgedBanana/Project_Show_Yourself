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
    }

    private void Start()
    {
        if (progressBar != null)
        {
            progressBar.interactable = false;
        }

        if (loadingCanva != null)
        {
            loadingCanva.SetActive(false);
        }

        StartCoroutine(InitializeSceneLogic());
    }

    private IEnumerator InitializeSceneLogic()
    {
        // Wait until the end of the frame so PlayerController.Instance can initialize
        yield return new WaitForEndOfFrame();

        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene != "Training_Facility")
        {
            PlayerController pc = PlayerController.Instance;

            if (pc != null) // Safety check to prevent the NullReferenceException
            {
                // Unlock all movement and weapons
                pc.primaryAuthorized = true;
                pc.sidearmAuthorized = true;
                pc.canMoveAtAll = true;
                pc.canSprintAuthorized = true;
                pc.canLeanAuthorized = true;

                // Unlock the Phone
                if (pc.phoneManager != null)
                {
                    pc.phoneManager.canOpenPhone = true;
                }

                // Unlock Live Fire for all weapons
                WeaponBase[] weapons = pc.GetComponentsInChildren<WeaponBase>(true);
                foreach (WeaponBase w in weapons)
                {
                    w.instructorTriggerAuth = true;
                }

                Debug.Log("GameManager: Non-Tutorial scene detected. All restrictions lifted.");
            }
            else
            {
                Debug.LogError("GameManager: Could not find PlayerController.Instance!");
            }
        }
    }

    public void StartLoadingScene(string sceneName)
    {
        _sceneToLoad = sceneName;

        if (loadingCanva != null)
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
            if (progressBar != null)
            {
                progressBar.value = operation.progress;
            }

            yield return null;

            if (operation.progress >= 0.9f)
            {
                operation.allowSceneActivation = true;
            }
        }

        loadingCanva?.SetActive(false);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MenuScene")
        {
            ShowMouse();
        }
        else
        {
            HideMouse();
        }
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
