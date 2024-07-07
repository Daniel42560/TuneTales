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
    FMOD.RESULT Result;
    FMOD.Studio.System FMOD_System;

    protected override void Start()
    {
        base.Start();
        InitializeMusic(FMODEvents.Instance.background_music);
        InitializeFMODAPI(); 
        dialogueCallback = new FMOD.Studio.EVENT_CALLBACK(DialogueEventCallback);

        FMOD.System test;
        FMOD.Factory.System_Create(out test);
        Sound test_sound;
        test.createSound(@"C:\Dokumente\Projekte\Programmieren\TuneTales\Musik\Noten\Piano", FMOD.MODE.DEFAULT, out test_sound);
    }

    private void InitializeFMODAPI()
    {
        Result = FMOD.Studio.System.create(out FMOD_System);
        if (Result != FMOD.RESULT.OK)
        {
            Console.WriteLine($"FMOD error! ({(int)Result}) {FMOD.Error.String(Result)}");
            Environment.Exit(-1);
        }
        // Initialize FMOD Studio, which will also initialize FMOD Core
        Result = FMOD_System.initialize(512, FMOD.Studio.INITFLAGS.NORMAL, FMOD.INITFLAGS.NORMAL, IntPtr.Zero);
        if (Result != FMOD.RESULT.OK)
        {
            Console.WriteLine($"FMOD error! ({(int)Result}) {FMOD.Error.String(Result)}");
            Environment.Exit(-1);
        }
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

    //- Nur Temporär
    FMOD.Studio.EVENT_CALLBACK dialogueCallback;

    public FMODUnity.EventReference EventName;

#if UNITY_EDITOR
    void Reset()
    {
        EventName = FMODUnity.EventReference.Find("event:/ProgrammerEvent");
    }
#endif

    void PlayDialogue(string key)
    {
        var dialogueInstance = FMODUnity.RuntimeManager.CreateInstance(EventName);

        // Pin the key string in memory and pass a pointer through the user data
        GCHandle stringHandle = GCHandle.Alloc(key);
        dialogueInstance.setUserData(GCHandle.ToIntPtr(stringHandle));

        dialogueInstance.setCallback(dialogueCallback);
        dialogueInstance.start();
        dialogueInstance.release();
    }

    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    static FMOD.RESULT DialogueEventCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
    {
        FMOD.Studio.EventInstance instance = new FMOD.Studio.EventInstance(instancePtr);

        // Retrieve the user data
        IntPtr stringPtr;
        instance.getUserData(out stringPtr);

        // Get the string object
        GCHandle stringHandle = GCHandle.FromIntPtr(stringPtr);
        String key = stringHandle.Target as String;

        switch (type)
        {
            case FMOD.Studio.EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND:
                {
                    FMOD.MODE soundMode = FMOD.MODE.LOOP_NORMAL | FMOD.MODE.CREATECOMPRESSEDSAMPLE | FMOD.MODE.NONBLOCKING;
                    var parameter = (FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES));

                    if (key.Contains("."))
                    {
                        FMOD.Sound dialogueSound;
                        var soundResult = FMODUnity.RuntimeManager.CoreSystem.createSound(Application.streamingAssetsPath + "/" + key, soundMode, out dialogueSound);
                        if (soundResult == FMOD.RESULT.OK)
                        {
                            parameter.sound = dialogueSound.handle;
                            parameter.subsoundIndex = -1;
                            Marshal.StructureToPtr(parameter, parameterPtr, false);
                        }
                    }
                    else
                    {
                        FMOD.Studio.SOUND_INFO dialogueSoundInfo;
                        var keyResult = FMODUnity.RuntimeManager.StudioSystem.getSoundInfo(key, out dialogueSoundInfo);
                        if (keyResult != FMOD.RESULT.OK)
                        {
                            break;
                        }
                        FMOD.Sound dialogueSound;
                        var soundResult = FMODUnity.RuntimeManager.CoreSystem.createSound(dialogueSoundInfo.name_or_data, soundMode | dialogueSoundInfo.mode, ref dialogueSoundInfo.exinfo, out dialogueSound);
                        if (soundResult == FMOD.RESULT.OK)
                        {
                            parameter.sound = dialogueSound.handle;
                            parameter.subsoundIndex = dialogueSoundInfo.subsoundindex;
                            Marshal.StructureToPtr(parameter, parameterPtr, false);
                        }
                    }
                    break;
                }
            case FMOD.Studio.EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND:
                {
                    var parameter = (FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES));
                    var sound = new FMOD.Sound(parameter.sound);
                    sound.release();

                    break;
                }
            case FMOD.Studio.EVENT_CALLBACK_TYPE.DESTROYED:
                {
                    // Now the event has been destroyed, unpin the string memory so it can be garbage collected
                    stringHandle.Free();

                    break;
                }
        }
        return FMOD.RESULT.OK;
    }

    protected override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //PlayDialogue("A4_piano");
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

