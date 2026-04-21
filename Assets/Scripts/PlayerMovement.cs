using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode; 

public class PlayerMovement : NetworkBehaviour 
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (!IsOwner) return;

        float moveX = 0f;
        float moveY = 0f;

        if (Keyboard.current.dKey.isPressed) moveX = 1f;
        if (Keyboard.current.aKey.isPressed) moveX = -1f;
        if (Keyboard.current.wKey.isPressed) moveY = 1f;
        if (Keyboard.current.sKey.isPressed) moveY = -1f;

        movement = new Vector2(moveX, moveY);
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        rb.linearVelocity = movement.normalized * moveSpeed;
    }
}