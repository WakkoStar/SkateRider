using UnityEngine;

[System.Serializable]
public class AudioTrack
{
    public string name;
    [Range(0f, 1f)]
    public float volume = 1;

    [Range(1f, 3.0f)]
    public float pitch = 1;

    public bool loop = false;

    public AudioClip audioClip;

    [HideInInspector]
    public AudioSource source;

}
