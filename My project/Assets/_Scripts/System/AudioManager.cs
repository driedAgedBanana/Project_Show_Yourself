using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySounds(AudioList soundEvent, Vector3 position)
    {
        // Create a temporary GameObject to play the sound at the specified position
        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = position;
        AudioSource source = tempGO.AddComponent<AudioSource>();

        soundEvent.Play(source);

        // Clean up the temporary GameObject after the sound has finished playing
        Destroy(tempGO, soundEvent.audioClip[0].length + 1f);
    }
}
