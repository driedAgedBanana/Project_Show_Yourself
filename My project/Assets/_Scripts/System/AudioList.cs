using UnityEngine;

public class AudioList : MonoBehaviour
{
    public AudioSource source;

    // This function will show up in your Animation Event dropdown
    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
            source.PlayOneShot(clip);
    }
}
