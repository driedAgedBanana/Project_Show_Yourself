using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Music_Radio : MonoBehaviour, IPlayerInteract
{
    [Header("Radio Settings")]
    [SerializeField] private AudioClip[] playlist;
    public AudioList buttonPressed;

    private AudioSource audioSource;
    private bool _isOn = false;
    private Coroutine _activeRadioRoutine; // Tracks the "Radio Station" logic

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Interact()
    {
        // Stop any pending transitions or active play-queues
        if (_activeRadioRoutine != null) StopCoroutine(_activeRadioRoutine);

        if (_isOn)
        {
            _activeRadioRoutine = StartCoroutine(HandleInteraction(false));
        }
        else
        {
            _activeRadioRoutine = StartCoroutine(HandleInteraction(true));
        }
    }

    private IEnumerator HandleInteraction(bool turnOn)
    {
        // 1. Play the physical button press sound
        AudioManager.Instance.PlaySounds(buttonPressed, transform.position);

        // 2. Wait for the interaction delay
        yield return new WaitForSeconds(0.6f);

        if (turnOn)
        {
            _isOn = true;
            // Start the infinite loop of music
            yield return StartCoroutine(MusicLoop());
        }
        else
        {
            TurnOff();
        }
    }

    private IEnumerator MusicLoop()
    {
        while (_isOn)
        {
            if (playlist.Length == 0) yield break;

            // Pick and Play
            int randomIndex = Random.Range(0, playlist.Length);
            audioSource.clip = playlist[randomIndex];
            audioSource.Play();

            Debug.Log($"Radio playing: {audioSource.clip.name}");

            // WAIT until the song is finished
            // We wait for the length of the clip, then add a tiny buffer (0.1s)
            yield return new WaitForSeconds(audioSource.clip.length);

            // The loop repeats here, picking a new song!
        }
    }

    private void TurnOff()
    {
        audioSource.Stop();
        _isOn = false;
        _activeRadioRoutine = null;
    }
}