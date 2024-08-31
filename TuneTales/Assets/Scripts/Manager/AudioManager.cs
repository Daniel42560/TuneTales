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
    public EventInstance CurrentMusicInstance { get; private set; }
    public EventReference FirstLevelMusic;
    public Dictionary<string, bool> Notes = new();

    //- Programmer Variables
    private EVENT_CALLBACK BackgroundMusicCallback;
    private Dictionary<string, EventInstance> LoadedInstances = new();
    GCHandle StringHandle;


    protected override void Start()
    {
        base.Start();

        //- Create Callback so it doesnt get cleand up by GC
        BackgroundMusicCallback = new FMOD.Studio.EVENT_CALLBACK(BackgroundMusicEventCallback);

        //- Initialize Background Music
        InitializeMusic(FirstLevelMusic, "d_4");
    }

    public void PlayDialogue(string key)
    {
        //- When we want to play, we'll just find the event instance again and start it.
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
        StringHandle = GCHandle.Alloc(key, GCHandleType.Pinned);
        CurrentMusicInstance.setUserData(GCHandle.ToIntPtr(StringHandle));
        CurrentMusicInstance.setCallback(BackgroundMusicCallback);
        CurrentMusicInstance.start();
        CurrentMusicInstance.release();
    }
    private void InitializeDictionary()
    {
        for (int i = 0; i < 2; i++)
        {
            //ToDo
        }
    }

    #region Programmer Event Functions

    public void AssignSoundToInstanceWithCallback(EventInstance instance, string key)
    {
        StringHandle = GCHandle.Alloc(key, GCHandleType.Pinned);
        instance.setUserData(GCHandle.ToIntPtr(StringHandle));
    }    

    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    private static FMOD.RESULT BackgroundMusicEventCallback(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
    {
        switch (type)
        {

            case FMOD.Studio.EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND:
                {
                    FMOD.MODE soundMode = FMOD.MODE.LOOP_NORMAL | FMOD.MODE.CREATECOMPRESSEDSAMPLE | FMOD.MODE.NONBLOCKING;
                    FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES parameter =
                        (FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr,
                                                        typeof(FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES));

                    string new_key = Helper.ConvertUtf8ToUtf16(parameter.name);
                    if (new_key.Contains('d'))
                    {
                        break;
                    }
                    if (Instance == null)
                    {
                        UnityEngine.Debug.Log("Instance ist null");
                    }
                    else if (new_key == null)
                    {
                        UnityEngine.Debug.Log("Key ist null lol");
                    }
                    else
                    {
                        //Instance.AssignSoundToInstanceWithCallback(Instance.CurrentMusicInstance, new_key);
                    }
                    FMOD.RESULT keyResult = RuntimeManager.StudioSystem.getSoundInfo(new_key, out FMOD.Studio.SOUND_INFO uiSoundInfo);
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
        }
        return FMOD.RESULT.OK;
    }
    #endregion
    private void OnDestroy()
    {
        var result = CurrentMusicInstance.setUserData(IntPtr.Zero);
        if (result != RESULT.OK)
        {
            UnityEngine.Debug.LogError(result.ToString());
        }
        result = CurrentMusicInstance.setCallback(null);
        if (result != RESULT.OK)
        {
            UnityEngine.Debug.LogError(result.ToString());
        }

        if (StringHandle.IsAllocated)
            StringHandle.Free();
    }
    protected override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //AssignSoundToProgrammer("a_4");
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //AssignSoundToProgrammer("e_4");
        }
    }
}

