using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{

    protected override void Start()
    {
        base.Start();
        InitializeMusic(FMODEvents.Instance.background_music);        
    }
    public void PlayOneShot(EventReference sound, Vector3 world_pos)
    {
        RuntimeManager.PlayOneShot(sound, world_pos);
    }
    public EventInstance CreateEventInstance(EventReference event_reference)
    {
        EventInstance event_instance = RuntimeManager.CreateInstance(event_reference);
        return event_instance;
    }

    private EventInstance MusicEventInstance;
    private void InitializeMusic(EventReference music_event_reference)
    {
        MusicEventInstance = CreateEventInstance(music_event_reference);
        MusicEventInstance.start();
    }
}

