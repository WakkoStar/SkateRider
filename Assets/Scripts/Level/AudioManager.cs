using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public AudioTrack[] audioTracks;

    public static AudioManager instance;

    void Awake()
    {

        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (var track in audioTracks)
        {
            track.source = gameObject.AddComponent<AudioSource>();
            track.source.clip = track.audioClip;
            track.source.volume = track.volume;
            track.source.pitch = track.pitch;
            track.source.loop = track.loop;
            track.source.spatialBlend = 0.7f;
            track.source.playOnAwake = false;
            track.source.spread = 360;
        }
    }

    public void PlayOnly(string trackName, string[] staticTracksNames, float pitch = 1.0f)
    {

        foreach (var staticTrackName in staticTracksNames)
        {
            if (staticTrackName == trackName)
            {
                Play(staticTrackName, pitch);
            }
            else
            {
                Stop(staticTrackName);
            }
        }
    }

    public void Play(string name, float pitch = 1.0f)
    {
        var track = audioTracks.First(track => track.name == name);
        track.source.pitch = pitch;
        if (!track.source.isPlaying) track.source.Play();
    }

    public void Stop(string name)
    {
        var track = audioTracks.First(track => track.name == name);
        if (track.source.isPlaying) track.source.Stop();
    }

    public void StopAll(string[] names)
    {
        foreach (var name in names)
        {
            Stop(name);
        }
    }

}

