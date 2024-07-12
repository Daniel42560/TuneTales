using FMOD;
using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    //public FMODUnity.EventReference EventName;

    private EventInstance CurrentMusicInstance;
    public EventReference TestEventReference;

    //- Programmer Variables
    private EVENT_CALLBACK BackgroundMusicCallback;
    private Dictionary<string, EventInstance> LoadedInstances = new();


    protected override void Start()
    {
        base.Start();

        //- Initialize Background Music
        BackgroundMusicCallback = BackgroundMusicEventCallback;
        InitializeMusic(TestEventReference, "d_4");

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
    private void InitializeMusic(EventReference music_reference, string key)
    {
        CurrentMusicInstance = RuntimeManager.CreateInstance(music_reference);
        GCHandle stringHandle = GCHandle.Alloc(key, GCHandleType.Pinned);
        CurrentMusicInstance.setUserData(GCHandle.ToIntPtr(stringHandle));
        CurrentMusicInstance.setCallback(BackgroundMusicCallback);
        CurrentMusicInstance.start();
        CurrentMusicInstance.release();
    }

    #region Programmer Event Functions
    public void AssignSoundToProgrammer(string key)
    {
        GCHandle stringHandle = GCHandle.Alloc(key, GCHandleType.Pinned);
        CurrentMusicInstance.setUserData(GCHandle.ToIntPtr(stringHandle));
    }
    public void AssignSoundToProgrammer(EventReference reference, string key)
    {
        EventInstance instance = RuntimeManager.CreateInstance(reference);
        GCHandle stringHandle2 = GCHandle.Alloc(key, GCHandleType.Pinned);
        instance.setUserData(GCHandle.ToIntPtr(stringHandle2));
        instance.setCallback(BackgroundMusicCallback);
    }    

    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    private static FMOD.RESULT BackgroundMusicEventCallback(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
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

                    AudioManager.Instance.AssignSoundToProgrammer(Helper.ConvertUtf8ToUtf16(parameter.name));

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

                        FMOD.RESULT soundResult = RuntimeManager.CoreSystem.createSound(uiSoundInfo.name_or_data, soundMode | uiSoundInfo.mode, ref uiSoundInfo.exinfo, out FMOD.Sound uiSound);
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

//#if UNITY_EDITOR
//    void Reset()
//    {
//        EventName = FMODUnity.EventReference.Find("event:/ProgrammerEvent");
//    }
//#endif

    #endregion

    protected override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AssignSoundToProgrammer("a_4");
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            AssignSoundToProgrammer("e_4");
        }
    }
}

