using UnityEngine;

public class BackTomainMenu : MonoBehaviour, IPlayerInteract
{
    public void Interact()
    {
        Time.timeScale = 1f;
        GameManager.Instance.ShowMouse();
        GameManager.Instance.StartLoadingScene("MenuScene");

        Music_Radio musicRadio = FindFirstObjectByType<Music_Radio>();

        if (musicRadio != null)
        {
            musicRadio.TurnOff();
        }
    }
}
