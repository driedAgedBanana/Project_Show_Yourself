using UnityEngine;

[CreateAssetMenu(menuName = "AudioData / Audio")]
public class AudioList : ScriptableObject
{
    public AudioClip[] audioClip; // Add multiple for random variation
    public float volume = 1f;
    public float pitchRange = 0.1f;

    public void Play(AudioSource source)
    {
        if (audioClip.Length == 0) return;

        source.clip = audioClip[Random.Range(0, audioClip.Length)];
        source.volume = volume;
        source.pitch = 1f + Random.Range(-pitchRange, pitchRange);
        source.Play();
    }
}
