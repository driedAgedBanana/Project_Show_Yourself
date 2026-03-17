using System.Collections;
using TMPro;
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

    [Header("UI")]
    public TextMeshProUGUI songNames;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {

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

            // 1. Pick a random index
            int randomIndex = Random.Range(0, playlist.Length);
            AudioClip selectedClip = playlist[randomIndex];

            // 2. Assign and Play
            audioSource.clip = selectedClip;
            audioSource.Play();

            // 3. Update the UI once (No foreach loop needed!)
            if (songNames != null)
            {
                songNames.text = $"Now playing: {selectedClip.name}";
            }

            Debug.Log($"Radio playing: {selectedClip.name}");

            // 4. Wait for the song to finish
            yield return new WaitForSeconds(selectedClip.length);
        }
    }

    private void TurnOff()
    {
        audioSource.Stop();
        _isOn = false;
        _activeRadioRoutine = null;

        // Clear the UI text when the radio is off
        if (songNames != null)
        {
            songNames.text = "Radio Off"; // Or set to string.Empty
        }
    }
}