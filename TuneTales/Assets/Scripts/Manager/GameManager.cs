using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    //- ToDo: Bug mit inkorrekter Mouse Position fixen

    [Header("Links")]
    public CinemachineBrain CinBrain;
    public PlayerController Player;

    //- Debug
    public AudioSource Audio;

    protected override void Start()
    {
        base.Start();
    }
}
