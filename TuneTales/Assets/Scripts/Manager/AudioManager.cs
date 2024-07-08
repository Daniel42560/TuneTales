using FMOD;
using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    public FMODUnity.EventReference EventName;

    private EVENT_CALLBACK DialogueCallback;
    private Dictionary<string, EventInstance> LoadedInstances = new();


    protected override void Start()
    {
        base.Start();
        DialogueCallback = DialogueEventCallback;
        InitializeMusic(FMODEvents.Instance.background_music);
        LoadSound(EventName, "A4_piano");
        LoadSound(EventName, "E4_piano");
    }

    public void PlayDialogue(string key)
    {
        // When we want to play, we'll just find the event instance again and start it.
        var inst = LoadedInstances[key];
        inst.start();
    }
    public void PlayOneShot(EventReference sound, Vector3 world_pos)
    {
        RuntimeManager.PlayOneShot(sound, world_pos);
    }
    public EventInstance CreateEventInstance(EventReference event_reference)
    {
        return RuntimeManager.CreateInstance(event_reference);        
    }

    private EventInstance MusicEventInstance;
    private void InitializeMusic(EventReference music_event_reference)
    {
        MusicEventInstance = CreateEventInstance(music_event_reference);
        MusicEventInstance.start();
    }

    #region Programmer Event Functions

    private struct SoundAndInfo
    {
        public Sound Sound;
        public SOUND_INFO SoundInfo;
    }
    public void LoadSound(EventReference eventReference, string key)
    {
        if (LoadedInstances.ContainsKey(key)) return;

        // First get the sound info for this key.
        SOUND_INFO dialogueSoundInfo;
        var keyResult = RuntimeManager.StudioSystem.getSoundInfo(key, out dialogueSoundInfo);
        if (keyResult != RESULT.OK)
        {
            UnityEngine.Debug.LogError($"Error when loading {key}: {keyResult}");
            return;
        }

        MODE soundMode =
            MODE.LOOP_NORMAL | // Was set like this in example code. Perhaps should be changed to FMOD_LOOP_OFF
            MODE.CREATECOMPRESSEDSAMPLE | // This loads the sound compressed into memory (uncompressed if it isn't compressed). FMOD_CREATESTREAM could load directly from file. // FMOD_CREATESAMPLE decompresses on load, optimized for playback
            MODE.NONBLOCKING; // Don't block the main thread. 

        // Create the sound with the createSound method.
        Sound dialogueSound;
        var soundResult = RuntimeManager.CoreSystem.createSound(
            dialogueSoundInfo.name_or_data,
            soundMode | dialogueSoundInfo.mode,
            ref dialogueSoundInfo.exinfo,
            out dialogueSound);

        if (soundResult != RESULT.OK)
        {
            UnityEngine.Debug.LogError($"Trying to load sound with key {key} returned error {soundResult}");
            return;
        }

        // We'll create a struct to hold both the Sound itself and SoundInfo data.
        var soundAndInfo = new SoundAndInfo()
        {
            Sound = dialogueSound,
            SoundInfo = dialogueSoundInfo
        };
        // Pin the struct to memory with GCHandle.Alloc
        GCHandle soundInfoHandle = GCHandle.Alloc(soundAndInfo, GCHandleType.Pinned);

        // Finally create an instance for the Event and set the userdata to point to the struct we just pinned.
        var dialogueInstance = RuntimeManager.CreateInstance(eventReference);
        dialogueInstance.setUserData(GCHandle.ToIntPtr(soundInfoHandle));
        // Set the callback.
        dialogueInstance.setCallback(DialogueCallback);
        // Store the EventInstance for when we're going to play the file.
        LoadedInstances.Add(key, dialogueInstance);
    }
    public void Dispose()
    {
        // Don't forget to release the saved instances. The sounds themselves will be destroyed with the callback.
        foreach (var inst in LoadedInstances.Values)
        {
            inst.release();
        }

        LoadedInstances.Clear();
    }

    [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
    static RESULT DialogueEventCallback(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
    {
        //UnityEngine.Debug.Log("Dialogue Callback with type " + type);
        try
        {
            // Get the EventInstance from the provided pointer
            var instance = new EventInstance(instancePtr);
            instance.getUserData(out IntPtr soundInfoPtr);
            // Get the pointer to the SoundAndInfo struct from the instance.
            var soundInfoHandle = GCHandle.FromIntPtr(soundInfoPtr);
            // Dereference it.
            var soundInfo = (SoundAndInfo)soundInfoHandle.Target;
            switch (type)
            {
                // This is called before playing the programmer sound.
                // FMOD expects us to fill the parameterPtr with the sound and soundInfo during this call.
                case EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND:
                    {
                        // Get the struct from the callback via the pointer.
                        var parameter =
                            (PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr,
                                typeof(PROGRAMMER_SOUND_PROPERTIES));
                        // Put in the sound and soundInfo
                        parameter.sound = soundInfo.Sound.handle;
                        parameter.subsoundIndex = soundInfo.SoundInfo.subsoundindex;
                        // Put the modified data back where we found it.
                        Marshal.StructureToPtr(parameter, parameterPtr, false);
                        break;
                    }
                // When the event is destroyed
                case EVENT_CALLBACK_TYPE.DESTROYED:
                    {
                        // Release the sound
                        soundInfo.Sound.release();
                        // Free the handle to our info struct
                        soundInfoHandle.Free();
                        break;
                    }
            }
        }
        catch (Exception e) // Getting exceptions in this callback WILL hang Unity, so we'll use a try/catch block
        {
            if (e is ArgumentException || e is InvalidOperationException)
            {
                return RESULT.OK;
            }

            throw e;
        }

        return RESULT.OK;
    }

#if UNITY_EDITOR
    void Reset()
    {
        EventName = FMODUnity.EventReference.Find("event:/ProgrammerEvent");
    }
#endif

    #endregion

    protected override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayDialogue("A4_piano");
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PlayDialogue("E4_piano");
        }
    }
}

