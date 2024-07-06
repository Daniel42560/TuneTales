using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class FMODEvents : Singleton<FMODEvents>
{
    [field: Header("Piano Notes")]
    [field: SerializeField] public EventReference c_4_piano { get; private set; }

    [field: Header("Music")]
    [field: SerializeField] public EventReference background_music { get; private set; }
}
