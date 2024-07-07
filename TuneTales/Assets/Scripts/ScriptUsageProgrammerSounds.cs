using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using FMODUnity;
using FMOD.Studio;
using FMOD;

class ScriptUsageProgrammerSounds : MonoBehaviour
{
    private EVENT_CALLBACK dialogueCallback;
    public FMODUnity.EventReference EventName;

    private Dictionary<string, EventInstance> _loadedInstances = new();

    public struct SoundAndInfo
    {
        public Sound Sound;
        public SOUND_INFO SoundInfo;
    }

#if UNITY_EDITOR
    void Reset()
    {
        EventName = FMODUnity.EventReference.Find("event:/ProgrammerEvent");
    }
#endif

    void Start()
    {
        // Explicitly create the delegate object and assign it to a member so it doesn't get freed
        // by the garbage collected while it's being used
        dialogueCallback = DialogueEventCallback;
        LoadSound(EventName, "A4_piano");
    }
    public void LoadSound(EventReference eventReference, string key)
    {
        if (_loadedInstances.ContainsKey(key)) return;

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
        dialogueInstance.setCallback(dialogueCallback);
        // Store the EventInstance for when we're going to play the file.
        _loadedInstances.Add(key, dialogueInstance);
    }
    public void Dispose()
    {
        // Don't forget to release the saved instances. The sounds themselves will be destroyed with the callback.
        foreach (var inst in _loadedInstances.Values)
        {
            inst.release();
        }

        _loadedInstances.Clear();
    }

    void PlayDialogue(string key)
    {
        // When we want to play, we'll just find the event instance again and start it.
        var inst = _loadedInstances[key];
        inst.start();
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


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayDialogue("A4_piano");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlayDialogue("640165main_Lookin At It");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            PlayDialogue("640169main_Press to ATO");
        }
    }
}
