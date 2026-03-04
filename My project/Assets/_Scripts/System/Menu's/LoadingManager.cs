using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance { get; private set; }

    public GameObject loadingCanva;
    public GameObject menuCanvas;

    [Header("UI elements")]
    public Slider progressBar;

    private string _sceneToLoad;

    private void Start()
    {
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
            Debug.LogWarning("Loading Canva is missing!");
        }

        StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(0.01f);

        loadingCanva?.SetActive(true); // Ensure UI stays visible
        menuCanvas?.SetActive(false);

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

        loadingCanva?.SetActive(false); // Only deactivate after loading is completed
    }
}
