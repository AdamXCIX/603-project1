﻿using System.Collections;
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
    //For spritesheet animations
    //http://www.strandedsoft.com/using-spritesheets-with-unity3d/

    [SerializeField] private float walkSpeed; //Walk Speed
    [SerializeField] private float jumpSpeed; //Initial Jump Speed
    [SerializeField] private float shotSpeed; //Projectile Speed
    [SerializeField] private float shotDistance; //Distance Projectile moves before dropping
    [SerializeField] private float shotDelay; //Seconds between Projectile shots
    [SerializeField] private float kbForce; //Knockback force
    [SerializeField] private float regenDelay; //Seconds between health regenerations
    [SerializeField] private int sizeStages; //Number of stages for size changes
    

    private bool canJump; //Player can jump
    private bool onGround; //Player is on ground
    private bool canShoot; //Player can shoot
    private bool canSwipe; //Player can swipe attack
    private bool canTakeDamage; //Player can be damaged
    private bool isDead;
    private bool facingRight;
    private float timeSinceRegen;


    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Rigidbody2D rigidbody2D;
    private BoxCollider2D boxCollider2D;
    private LayerMask groundLayer;
    private PlayerState state;
    private PlayerState prevState;

    public float MHealth
    {
        get { return MaxHealth; }
    }
    public float Health
    {
        get { return health; }
    }
    public float Damage
    {
        get { return damage; }
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        canShoot = true;
        canSwipe = true;
        canTakeDamage = true;

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        groundLayer = LayerMask.GetMask("Ground");
        state = PlayerState.Stand;
        prevState = state;

        rigidbody2D.freezeRotation = true; //Prevents player from rotating

        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (!isDead)
        {
            onGround = CheckIfOnGround(); //Check whether player is on ground

            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) && spriteRenderer.flipX) //Player Faces Left
                spriteRenderer.flipX = false;
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) && !spriteRenderer.flipX) //Player Faces Left
                spriteRenderer.flipX = true;

            if (Input.GetKey(KeyCode.J) && canShoot && (health > damage)) //Player can only shoot if it won't kill them
                Shoot();
            if (Input.GetKey(KeyCode.L) && canSwipe) //Melee attack
                Swipe();

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
                        Jump(jumpSpeed);
                    }
                    if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) //Player Moves Left
                        Move(-walkSpeed);
                    else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) //Player Moves Right
                        Move(walkSpeed);

                    if (onGround && rigidbody2D.velocity.y < 0) //Landing on the Ground
                        state = PlayerState.Stand;
                    break;
            }

            prevState = state; //Updates previous state
            UpdateAnimation();
            
            if (health != MaxHealth)
            {
                RegenerateHealth();
            }
        }
    }

    //------------------------Basic Controls------------------------
    private void Move(float speed) //Moves the player horizontally
    {
        rigidbody2D.velocity = new Vector2(speed, rigidbody2D.velocity.y);
    }

    private void Jump(float speed) //Allows the player to jump
    {
        //rigidbody2D.AddForce(new Vector2(0, force), ForceMode2D.Impulse);
        rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, speed);
    }

    protected override void Shoot() //Ranged Attack
    {
        ChangeHealth(-damage);

        //Set thread to turn shooting back on in a short amount of time

        Vector3 pos = gameObject.transform.position;
        pos.y -= 0.1f * transform.localScale.y;
        pos.z = 1;

        GameObject projectile = Instantiate(projectilePrefab, pos, gameObject.transform.rotation); //Creates a projectile and removes its health from the player
        projectile.transform.localScale = transform.localScale;
        PlayerProjectile projScript = projectile.GetComponent<PlayerProjectile>();
        if (projScript)
        {
            projScript.Damage = damage; //Set shot damage
            projScript.Distance = shotDistance; //Set shot damage
            if (!spriteRenderer.flipX) //Set shot direction
                projScript.Speed = shotSpeed * Vector2.right;
            else
                projScript.Speed = shotSpeed * Vector2.left;
        }

        StartCoroutine(DelayNextShot(shotDelay));
    }




    /************************************************************************************************
     * 
     * Melee Attack
     * 
     * ************************************************************************************************/
    protected void Swipe() //Melee Attack
    {
        
    }




    IEnumerator DelayNextShot(float shotDelay)
    {
        canShoot = false;
        float shotLength = 0; //Time since last shot

        while (shotLength < shotDelay)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            shotLength += Time.deltaTime;
        }
        canShoot = true;
    }

    //------------------------Animation------------------------
    private void UpdateAnimation()
    {
        if (!canShoot) //Player is using ranged attack
            animator.SetInteger("AnimState", 3);
        else if (!canSwipe) //Player is melee attack
            animator.SetInteger("AnimState", 2);
        else if (state == PlayerState.Walk) //Player is walking
            animator.SetInteger("AnimState", 1);
        else if (state == PlayerState.Stand || state == PlayerState.Jump) //Player is not attacking or walking
            animator.SetInteger("AnimState", 0);
    }

    private void FlipSprite() //Flips Sprite when player turns left or right
    {
        facingRight = !facingRight;
        Vector2 scale = rigidbody2D.transform.localScale;
        scale.x *= -1;
        rigidbody2D.transform.localScale = scale;
    }


    //------------------------Collision Handling------------------------
    private void OnCollisionEnter2D(Collision2D collision) //Handles collisions between player and physical GameObjects
    {
        //------Collision Handling is commented out until scripts are created------

        /*GameObject other = collision.gameObject;
        if (other.tag == "Enemy" && canTakeDamage) //Player touches an enemy
        {
            Enemy enemyScript = other.GetComponent<Enemy>();
            if (enemyScript) //Player takes damage from contact with enemy
            {
                Vector2 collisionDirection = transform.position - other.transform.position;
                TakeDamage(enemyScript.Damage, collisionDirection);
            }
        }
        else if (other.tag == "EnemyProjectile" && canTakeDamage) //Player touches an enemy's projectile
        {
            EnemyProjectile projScript = other.GetComponent<projectile>();
            if (projScript) //Player takes damage from contact with enemy's projectile
            {
                Vector2 collisionDirection = transform.position - other.transform.position;
                TakeDamage(projScript.Damage, collisionDirection);
                Destroy(other);
            }
        }*/
}

private void OnTriggerEnter2D(Collider2D collision) //Handles collisions between player and non-physical GameObjects
    {
        GameObject other = collision.gameObject;
        if (other.tag == "Pickup") //Player touches a health pickup
        {
            HealthPickup healthScript = other.GetComponent<HealthPickup>();
            if (healthScript) //Restores health and removes pickup
            {
                ChangeHealth(healthScript.Health);
                Destroy(other);
            }
        }
    }

    private bool CheckIfOnGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, boxCollider2D.size.y / 2 + 0.05f, groundLayer);
        return hit.collider != null;
    }

    //------------------------Health and Damage------------------------
    private void ChangeHealth(float value) //Increases and Decreases player's health
    {
        
        if ((health + value) > MaxHealth) //Health raised to max
            health = MaxHealth;
        else if ((health + value) <= 0) //Health lowered to 0
        {
            health = 0;
            isDead = true;
        }
        else //Raises or lowers health
            health += value;

        if (health > damage) //Changes color to show whetehr player can shoot
            spriteRenderer.color = Color.white;
        else if (health > 0)
            spriteRenderer.color = Color.gray;
        else
            spriteRenderer.color = new Color(0.25f, 0.25f, 0.25f);


        for (int i = sizeStages - 1; i >= 0; i--) //Changes size based on health
        {
            if (health >= (MaxHealth * i / sizeStages))
            {
                float percent = (i + 1.0f) / sizeStages;
                transform.localScale = new Vector3(percent, percent, 1);
                break;
            }
        }
    }

    void RegenerateHealth() //Regenerates health over time to prevent player from being forced to die
    {
        if (timeSinceRegen >= regenDelay) //Time delay has passed
        {
            health += 1;
            timeSinceRegen = 0;
            Debug.Log("Health: " + health);
        }
        else //Increment time
        {
            timeSinceRegen += Time.deltaTime;
        }
    }



    /************************************************************************************************
     * 
     * Restore small amount of health over time
     * 
     * ************************************************************************************************/





    public void TakeDamage(float value, Vector2 kbDirection) //Decreases player's health and Handles knockback
    {
        ChangeHealth(-value);
        
        if (health > 0)
        {
            StartCoroutine(Flash(1.0f, 0.05f));
            StartCoroutine(TakeKnockBack(0.1f, kbDirection));
        }
        
    }

    //------------------------Damage Indicators------------------------
    IEnumerator Flash(float flashDuration, float flashDelay)
    {
        canTakeDamage = false;

        float flashLength = 0; //Time player has been flashing
        Color temp = spriteRenderer.color; //Temporary color used to change player's alpha

        while (flashLength < flashDuration)
        {
            temp.a = 0f;
            spriteRenderer.color = temp;
            yield return new WaitForSeconds(flashDelay);
            temp.a = 255f;
            spriteRenderer.color = temp;
            yield return new WaitForSeconds(flashDelay);
            flashLength += flashDelay * 2;
        }
        canTakeDamage = true;
    }

    IEnumerator TakeKnockBack(float kbDuration, Vector2 kbDirection)
    {
        float kbLength = 0; //Time player has been flashing
        Color temp = spriteRenderer.color; //Temporary color used to change player's alpha

        while (kbLength < kbDuration)
        {
            rigidbody2D.velocity = kbDirection.normalized * kbForce;
            yield return new WaitForSeconds(Time.deltaTime);
            kbLength += Time.deltaTime;
        }
    }
}
