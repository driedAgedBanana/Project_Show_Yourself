using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource audioSource;

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

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySounds(AudioList soundEvent, Vector3 position)
    {
        GameObject tempGO = new GameObject("TempAudio_" + soundEvent.name);
        tempGO.transform.position = position;

        AudioSource source = tempGO.AddComponent<AudioSource>();

        // --- The Secret Sauce for 3D Sound ---
        source.spatialBlend = 1.0f; // 0 = 2D, 1 = 3D
        source.minDistance = 1f;    // Full volume within 1 meter
        source.maxDistance = 20f;   // Completely silent after 20 meters
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        // -------------------------------------

        soundEvent.Play(source);

        // Using soundEvent.audioClip[0].length is a good start, 
        // but remember if you have multiple clips of different lengths, 
        // you might want to pass the specific clip length to the Destroy call.
        Destroy(tempGO, soundEvent.audioClip[0].length + 1f);
    }
}
