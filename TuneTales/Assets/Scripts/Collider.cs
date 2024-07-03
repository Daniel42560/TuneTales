using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collider : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(TestCoroutine());
    }
    private IEnumerator TestCoroutine()
    {
        yield return new WaitForSeconds(2f);
        AudioManager.Instance.PlayNote(NoteSymbol.C, 4, Instrument.Synth);
        yield return new WaitForSeconds(0.5f);
        AudioManager.Instance.PlayNote(NoteSymbol.A, 4, Instrument.Synth);
        yield return new WaitForSeconds(1.5f);
        AudioManager.Instance.PlayNote(NoteSymbol.C, 4, Instrument.Synth);
        AudioManager.Instance.PlayNote(NoteSymbol.A, 4, Instrument.Synth);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Projectile")
        {
            AudioManager.Instance.PlayNote(NoteSymbol.C, 4, Instrument.Synth);
            AudioManager.Instance.PlayNote(NoteSymbol.A, 4, Instrument.Synth);
        }
    }
}
