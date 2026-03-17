using System.Collections;
using System.Collections.Generic; // Added for Lists
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
    private Coroutine _activeRadioRoutine;

    // Shuffle logic variables
    private List<AudioClip> _shuffleBag = new List<AudioClip>();
    private AudioClip _lastPlayedClip;

    [Header("UI")]
    public TextMeshProUGUI songNames;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Interact()
    {
        if (_activeRadioRoutine != null) StopCoroutine(_activeRadioRoutine);

        if (_isOn)
            _activeRadioRoutine = StartCoroutine(HandleInteraction(false));
        else
            _activeRadioRoutine = StartCoroutine(HandleInteraction(true));
    }

    private IEnumerator HandleInteraction(bool turnOn)
    {
        AudioManager.Instance.PlaySounds(buttonPressed, transform.position);
        yield return new WaitForSeconds(0.6f);

        if (turnOn)
        {
            _isOn = true;
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

            // 1. Get the next unique song
            AudioClip selectedClip = GetNextShuffleClip();

            if (selectedClip == null) yield break;

            // 2. Assign and Play
            audioSource.clip = selectedClip;
            audioSource.Play();
            _lastPlayedClip = selectedClip;

            // 3. Update UI
            if (songNames != null)
                songNames.text = $"Now playing: {selectedClip.name.Replace("_", " ")}";

            // 4. Wait for song to finish
            yield return new WaitForSeconds(selectedClip.length);
        }
    }

    private AudioClip GetNextShuffleClip()
    {
        // If the bag is empty, refill it from the main playlist
        if (_shuffleBag.Count == 0)
        {
            _shuffleBag.AddRange(playlist);
            ShuffleList(_shuffleBag);

            // EXTRA POLISH: If the first song in the new shuffle is the same 
            // as the one we just finished, move it to the end of the list.
            if (_shuffleBag.Count > 1 && _shuffleBag[0] == _lastPlayedClip)
            {
                AudioClip duplicate = _shuffleBag[0];
                _shuffleBag.RemoveAt(0);
                _shuffleBag.Add(duplicate);
            }
        }

        // Pull the top card from the deck
        AudioClip clipToPlay = _shuffleBag[0];
        _shuffleBag.RemoveAt(0);
        return clipToPlay;
    }

    // Fisher-Yates Shuffle Algorithm
    private void ShuffleList(List<AudioClip> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            AudioClip temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private void TurnOff()
    {
        audioSource.Stop();
        _isOn = false;
        _activeRadioRoutine = null;

        if (songNames != null)
            songNames.text = "Radio Off";
    }
}