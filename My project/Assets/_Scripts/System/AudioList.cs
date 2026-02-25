using UnityEngine;

public class AudioList : MonoBehaviour
{
    public AudioSource source;

    private void Start()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
    }

    // This function will show up in your Animation Event dropdown
    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
            source.PlayOneShot(clip);
    }
}
