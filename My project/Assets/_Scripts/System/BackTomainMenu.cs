using UnityEngine;

public class BackTomainMenu : MonoBehaviour, IPlayerInteract
{
    public void Interact()
    {
        Time.timeScale = 1f;
        GameManager.Instance.ShowMouse();
        GameManager.Instance.StartLoadingScene("MenuScene");
    }
}
