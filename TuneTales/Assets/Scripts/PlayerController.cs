using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float WalkSpeed = 5f;

    Vector2 MoveInput;
    Rigidbody2D RigBody;
    public bool IsMoving { get; private set; }

    private void Awake()
    {
        RigBody = GetComponent<Rigidbody2D>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        RigBody.velocity = new Vector2(MoveInput.x * WalkSpeed, RigBody.velocity.y);
    }
    #region Movement
    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();

        IsMoving = MoveInput != Vector2.zero;
    }
    public void OnFire(InputAction.CallbackContext context)
    {
        AudioManager.Instance.PlayNote(NoteSymbol.C, 4);
        Debug.Log("Fire");
    }
    #endregion
}
