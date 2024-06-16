using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note 
{
    public  NoteSymbol Symbol;
    public int Octave;
    public AudioClip Clip;

    public Note(NoteSymbol symbol, int octave, AudioClip sound)
    {
        Symbol = symbol;
        Octave = octave;
        Clip = sound;
    }
}
public enum NoteSymbol
{
    C, Cis, D, Dis, E, F, Fis, G, Gis, A, Ais, B
}
