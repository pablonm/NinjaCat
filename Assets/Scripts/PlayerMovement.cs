using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{
    public float speed;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;
    private PlayerHealth playerHealth;

    [SyncVar]
    private bool flipSprite;

	private void Start ()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        playerHealth = GetComponent<PlayerHealth>();
    }
	
    private void Move()
    {
        if (isLocalPlayer)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            if (horizontal != 0 || vertical != 0)
            {
                CmdMove(horizontal, vertical);
            }
        }
    }

    [Command]
    private void CmdMove(float horizontal, float vertical)
    {
        if (playerHealth.isAlive)
        {
            rb.velocity = new Vector3(horizontal, vertical) * speed;
            anim.SetFloat("speed", rb.velocity.magnitude);
        }
    }

    private void FlipSprite()
    {
        sprite.flipX = flipSprite;
        if (isLocalPlayer)
        {
            bool shouldFlip = (Camera.main.ScreenToWorldPoint(Input.mousePosition).x < transform.position.x);
            if (shouldFlip != flipSprite)
            {
                CmdFlipSprite(shouldFlip);
            }
        }
    }

    [Command]
    private void CmdFlipSprite(bool flip)
    {
        if (playerHealth.isAlive)
        {
            flipSprite = flip;
        }
    }

    private void Update()
    {
        Move();
        FlipSprite();
    }
}
