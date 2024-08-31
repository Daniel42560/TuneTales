using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note 
{
    public  NoteSymbol Symbol;
    public int Octave;
    public bool Mute;

    public Note(NoteSymbol symbol, int octave)
    {
        Symbol = symbol;
        Octave = octave;  
    }
}
public enum NoteSymbol
{
    C, Cis, D, Dis, E, F, Fis, G, Gis, A, Ais, B
}
public enum Instrument
{
    Synth, Piano
}
