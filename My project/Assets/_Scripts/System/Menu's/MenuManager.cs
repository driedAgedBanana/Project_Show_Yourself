using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

public enum MenuType
{
    Main,
    TutorialAsk,
    Credit
}

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Header("Menus")]
    public GameObject mainMenu;
    public GameObject tutorialAskMenu;
    public GameObject CreditMenu;


    private Dictionary<MenuType, GameObject> _menuCanvas = new Dictionary<MenuType, GameObject>();
    private MenuType _currentMenu = MenuType.Main;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Store the menu canvases in a dictionary for easy access
        _menuCanvas[MenuType.Main] = mainMenu;
        _menuCanvas[MenuType.TutorialAsk] = tutorialAskMenu;
        _menuCanvas[MenuType.Credit] = CreditMenu;

        foreach (GameObject menu in _menuCanvas.Values)
        {
            menu.SetActive(false);
        }

        SetMenuType(MenuType.Main);

    }

    public void SetMenuType(MenuType type)
    {
        // Changes the current meny type and activate the corresponding menu canvas
        // Disable the one that is not chosen
        foreach (GameObject menu in _menuCanvas.Values)
        {
            menu.SetActive(false);
        }

        // Enable the chosen menu
        if (_menuCanvas.ContainsKey(type))
        {
            _menuCanvas[type].SetActive(true);
        }

        _currentMenu = type;
    }

    public void ToMain()
    {
        SetMenuType(MenuType.Main);
    }

    public void ToTutorialAsk()
    {
        SetMenuType(MenuType.TutorialAsk);
    }

    public void ToCredit()
    {
        SetMenuType(MenuType.Credit);
    }

    public void ExitGame()
    {
        GameManager.Instance.QuitGame();
    }
}
