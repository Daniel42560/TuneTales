using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float MoveSpeed = 10f;

    Rigidbody2D RigBody;
    private void Awake()
    {
        RigBody = GetComponent<Rigidbody2D>();
    }
    // Start is called before the first frame update
    void Start()
    {      
        //- Movement
        Vector3 mouse_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse_pos.z = 0;
        Vector2 move_direction = (mouse_pos - transform.position).normalized;
        RigBody.velocity = move_direction * MoveSpeed;

        //- Sound
        //AudioManager.Instance.PlayOneShot(FMODEvents.Instance.c_4_piano, transform.position);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
    }
}
