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
    }

    private void ApplySceneRules(string sceneName)
    {
        PlayerController pc = PlayerController.Instance;

        if (pc == null)
        {
            Debug.LogError("GameManager: PlayerController not found!");
            return;
        }

        // DO NOT touch anything in training scene
        if (sceneName == "Training_Facility")
        {
            Debug.Log("GameManager: Training scene → TutorialManager has full control.");
            return;
        }

        // For ALL other scenes → fully unlock player
        pc.primaryAuthorized = true;
        pc.sidearmAuthorized = true;
        pc.canMoveAtAll = true;
        pc.canSprintAuthorized = true;
        pc.canLeanAuthorized = true;
        pc.isInRestrictedArea = false;

        if (pc.phoneManager != null)
            pc.phoneManager.canOpenPhone = true;

        WeaponBase[] weapons = pc.GetComponentsInChildren<WeaponBase>(true);
        foreach (WeaponBase w in weapons)
        {
            w.instructorTriggerAuth = true;
        }

        Debug.Log("GameManager: Non-training scene → unrestricted.");
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
                yield return new WaitForSeconds(0.2f); // give systems time
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

        ApplySceneRules(scene.name);
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
