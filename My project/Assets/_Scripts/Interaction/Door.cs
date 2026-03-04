using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Door : MonoBehaviour, IPlayerInteract
{
    public GameObject door;
    public BoxCollider doorCollider;
    public float moveTime = 1f;
    private Vector3 _newPosition;
    // public Volume doorVFX;
    public GameObject missionCompletePanel;


    public GameObject player;

    [Space]
    public AudioSource doorAudioSource;
    public AudioClip staticNoise;

    [Space]
    [SerializeField] private float referenceDistance = 5f;
    private Transform _playerListener;
    private float _distance;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Start()
    {
        doorCollider = GetComponent<BoxCollider>();
        if (doorCollider != null)
        {
            doorCollider.isTrigger = false;
        }

        // doorVFX = GetComponentInChildren<Volume>();

        doorAudioSource = GetComponent<AudioSource>();

        if(missionCompletePanel != null)
        {
            missionCompletePanel.SetActive(false);
        }
        
    }

    private void Update()
    {
        UpdateProximityEffects();
    }

    public void Interact()
    {
        missionCompletePanel.SetActive(true);
        GameManager.Instance.PauseGame();
        StopStaticAudio();
    }

    public void PlayStaticAudio()
    {
        if (doorAudioSource != null && staticNoise != null && !doorAudioSource.isPlaying)
        {
            doorAudioSource.clip = staticNoise;
            doorAudioSource.loop = true;
            doorAudioSource.Play();
        }
    }

    public void StopStaticAudio()
    {
        if (doorAudioSource != null && doorAudioSource.isPlaying)
        {
            doorAudioSource.Stop();
        }
    }

    private void UpdateProximityEffects()
    {
        if (player == null || doorAudioSource == null) return;

        _distance = Vector3.Distance(transform.position, player.transform.position);

        float intensity = 1f - Mathf.Clamp01(_distance / referenceDistance);

        doorAudioSource.volume = intensity;

        //if (doorVFX != null && doorVFX.profile != null)
        //{
        //    doorVFX.weight = intensity;
        //}
    }
}
