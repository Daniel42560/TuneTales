using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    public Note[,] Notes;
    public AudioClip[] Clips = new AudioClip[96];
    public AudioSource NoteSource, MusicSource;

    public AudioClip DebugClip;

    protected override void Awake()
    {
        base.Awake();
        FillNotesMDA();
    }

    public void PlayMusic(string name)
    {
        //Note note = Array.Find(Notes, x => x.Name == name);
        //if (note == null)
        //{
        //    Debug.Log("Sound not found");
        //}
        //else
        //{
        //    NoteSource.clip = note.Clip;
        //    NoteSource.loop = true;
        //    NoteSource.Play();
        //}
    }
    public void PlayNote(NoteSymbol symbol, int octave)
    {
        if (octave <= 0)
        {
            Debug.Log("Octave cant be lower than zero");
            return;
        }

        Note note = Notes[octave - 1, (int)symbol];
        if (note.Clip == null)
            Debug.Log("Clip is null");
        else
        {
            NoteSource.clip = note.Clip;
            NoteSource.loop = true;
            NoteSource.Play();
        }
    }
    private void FillNotesMDA()
    {
        Notes = new Note[8, 12];
        for (int i = 0; i < Notes.GetLength(0); i++)
        {
            for (int j = 0; j < Notes.GetLength(1); j++)
            {
                Notes[i, j] = new Note((NoteSymbol)j, i, Clips[i * 12 + j]);
            }
        }
    }
}

