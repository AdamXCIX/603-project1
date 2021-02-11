using System.Collections;
using System.Collections.Generic;
using UnityEngine;
enum PlayerState
{
    Stand,
    Walk,
    Jump,
}

public class Player : Character
{
    #region Fields
    [SerializeField] private float jumpForce;
    [SerializeField] private LayerMask groundLayer;

    private PlayerState state;
    private PlayerState prevState;

    private bool canJump;
    private bool onGround;
    #endregion 


    // Start is called before the first frame update
    protected override void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        state = PlayerState.Stand;

        base.Start(); //Function isn't virtual yet
    }

    // Update is called once per frame
    protected override void Update()
    {
        onGround = CheckIfOnGround(); //Set whether player is on ground

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) && spriteRenderer.flipX) //Player Faces Left
            spriteRenderer.flipX = false;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) && !spriteRenderer.flipX) //Player Faces Left
            spriteRenderer.flipX = true;

        switch (state)
        {
            //Player is not moving
            case PlayerState.Stand:
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) //Player Walks
                    state = PlayerState.Walk;
                else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) //Player Walks
                    state = PlayerState.Walk;
                else if (Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.Space)) //Player Jumps
                {
                    canJump = true;
                    state = PlayerState.Jump;
                }
                break;

            //Player is moving
            case PlayerState.Walk:
                if (Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.Space)) //Player Jumps
                {
                    canJump = true;
                    state = PlayerState.Jump;
                }
                else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) //Player Moves Left
                    Move(-walkSpeed);
                else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) //Player Moves Right
                    Move(walkSpeed);
                else
                    state = PlayerState.Stand;

                break;

            //Player is jumping
            case PlayerState.Jump:
                if (onGround && canJump) //Jump for one frame if not on ground
                {
                    canJump = false; 
                    Jump(jumpForce);
                }
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) //Player Moves Left
                    Move(-walkSpeed);
                else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) //Player Moves Right
                    Move(walkSpeed);

                if (onGround && rigidBody.velocity.y < 0) //Landing on the Ground
                    state = PlayerState.Stand;
                break;
        }

        prevState = state; //Updates previous state
    }

    private void Jump(float speed) //Allows the player to jump
    {
        //rigidbody2D.AddForce(new Vector2(0, force), ForceMode2D.Impulse);
        rigidBody.velocity = new Vector2(rigidBody.velocity.x, speed);
    }

    private bool CheckIfOnGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, boxCollider2D.size.y / 2 + 0.5f, groundLayer);
        return hit.collider != null;
    }
}
