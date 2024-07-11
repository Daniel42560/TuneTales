using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using FMODUnity;
using System.Runtime.InteropServices;
using FMOD;
using FMOD.Studio;

public class ProgrammerTutorialScript : MonoBehaviour
{
    [SerializeField]
    private EventReference uiProgrammerSoundEvent;
    private FMOD.Studio.EVENT_CALLBACK uiCallback;
    private EventInstance CurrentMusicInstance;

    private void Start()
    {
        CurrentMusicInstance = CreateEventInstance(FMODEvents.Instance.background_music);
        uiCallback = new FMOD.Studio.EVENT_CALLBACK(UIEventCallback);
        InitializeMusic(FMODEvents.Instance.background_music, "A4_piano");
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AssignSoundToProgrammer("A4_piano");
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            AssignSoundToProgrammer("E4_piano");
        }
    }
    private void InitializeMusic(EventReference music_event_reference, string key)
    {
        GCHandle stringHandle = GCHandle.Alloc(key, GCHandleType.Pinned);
        CurrentMusicInstance.setUserData(GCHandle.ToIntPtr(stringHandle));
        CurrentMusicInstance.setCallback(uiCallback);
        CurrentMusicInstance.start();
        //AssignSoundToProgrammer("A4_piano");
    }
    public EventInstance CreateEventInstance(EventReference event_reference)
    {
        return RuntimeManager.CreateInstance(event_reference);
    }
    public void AssignSoundToProgrammer(string key)
    {
        GCHandle stringHandle = GCHandle.Alloc(key, GCHandleType.Pinned);
        CurrentMusicInstance.setUserData(GCHandle.ToIntPtr(stringHandle));
    }
    public void PlayUISoundByKey(string key)
    {
        FMOD.Studio.EventInstance uiSoundInstance = RuntimeManager.CreateInstance(uiProgrammerSoundEvent);

        GCHandle stringHandle = GCHandle.Alloc(key, GCHandleType.Pinned);
        uiSoundInstance.setUserData(GCHandle.ToIntPtr(stringHandle));

        uiSoundInstance.setCallback(uiCallback);
        uiSoundInstance.start();
        uiSoundInstance.release();
        
    }

    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    private static FMOD.RESULT UIEventCallback(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
    {
        FMOD.Studio.EventInstance instance = new FMOD.Studio.EventInstance(instancePtr);
        instance.getUserData(out IntPtr stringPtr);

        GCHandle stringHandle = GCHandle.FromIntPtr(stringPtr);
        String key = stringHandle.Target as String;

        switch (type)
        {

            case FMOD.Studio.EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND:
                {
                    FMOD.MODE soundMode = FMOD.MODE.LOOP_NORMAL | FMOD.MODE.CREATECOMPRESSEDSAMPLE | FMOD.MODE.NONBLOCKING;
                    FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES parameter = 
                        (FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr, 
                                                        typeof(FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES));

                    if (key.Contains("."))
                    {
                        FMOD.RESULT soundResult = RuntimeManager.CoreSystem.createSound(Application.streamingAssetsPath + "/" + key, soundMode, out FMOD.Sound uiSound);
                        if (soundResult == FMOD.RESULT.OK)
                        {
                            parameter.sound = uiSound.handle;
                            parameter.subsoundIndex = -1;
                            Marshal.StructureToPtr(parameter, parameterPtr, false);
                        }
                    }
                    else
                    {
                        FMOD.RESULT keyResult = RuntimeManager.StudioSystem.getSoundInfo(key, out FMOD.Studio.SOUND_INFO uiSoundInfo);
                        if (keyResult != FMOD.RESULT.OK)
                        {
                            break;
                        }

                        FMOD.RESULT soundResult = RuntimeManager.CoreSystem.createSound(uiSoundInfo.name_or_data, soundMode | uiSoundInfo.mode,ref uiSoundInfo.exinfo, out FMOD.Sound uiSound);
                        if (soundResult == FMOD.RESULT.OK)
                        {
                            parameter.sound = uiSound.handle;
                            parameter.subsoundIndex = uiSoundInfo.subsoundindex;
                            Marshal.StructureToPtr(parameter, parameterPtr, false);
                        }
                    }
                    break;
                }
            case FMOD.Studio.EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND:
                {
                    FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES parameter =
                        (FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr,
                                                        typeof(FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES));
                    FMOD.Sound sound = new FMOD.Sound(parameter.sound);
                    sound.release();
                    break;
                }
            case FMOD.Studio.EVENT_CALLBACK_TYPE.DESTROYED:
                {
                    stringHandle.Free();                
                    break;
                }
        }
        return FMOD.RESULT.OK;
    }
}
