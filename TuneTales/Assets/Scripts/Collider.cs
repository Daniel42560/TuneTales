using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collider : MonoBehaviour
{
    [SerializeField]
    private string ActivatedNote;
    private void Start()
    {

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Projectile")
        {
            AudioManager.Instance.SetNote(ActivatedNote, true);
        }
    }
}
