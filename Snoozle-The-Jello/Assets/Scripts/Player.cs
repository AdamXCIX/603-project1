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
    [SerializeField] private float jumpSpeed; //Initial Jump Speed
    [SerializeField] private float shotSpeed; //Projectile Speed
    [SerializeField] private float shotDistance; //Distance Projectile moves before dropping
    [SerializeField] private float shotDelay; //econds between Projectile shots
    [SerializeField] private float kbForce; //Knockback force
    [SerializeField] private int sizeStages; //Number of stages for size changes
    

    private bool canJump; //Player can jump
    private bool onGround; //Player is on ground
    private bool canShoot; //Player can shoot
    private bool canTakeDamage; //Player can be damaged

    private BoxCollider2D boxCollider;
    private LayerMask groundLayer;
    private PlayerState state;
    private PlayerState prevState;


    // Start is called before the first frame update
    protected override void Start()
    {
        canShoot = true;
        canTakeDamage = true;

        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        groundLayer = LayerMask.GetMask("Ground");
        state = PlayerState.Stand;
        prevState = state;

        rigidBody.freezeRotation = true; //Prevents player from rotating

        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        onGround = CheckIfOnGround(); //Check whether player is on ground

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) && spriteRenderer.flipX) //Player Faces Left
            spriteRenderer.flipX = false;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) && !spriteRenderer.flipX) //Player Faces Left
            spriteRenderer.flipX = true;

        if (Input.GetKey(KeyCode.J) && canShoot)
        {
            Shoot();
        }

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

                if (onGround && base.rigidBody.velocity.y < 0) //Landing on the Ground
                    state = PlayerState.Stand;
                break;
        }

        prevState = state; //Updates previous state
    }

    //------------------------Basic Controls------------------------
    private void Jump(float speed) //Allows the player to jump
    {
        //rigidbody2D.AddForce(new Vector2(0, force), ForceMode2D.Impulse);
        base.rigidBody.velocity = new Vector2(base.rigidBody.velocity.x, speed);
    }

    protected override void Shoot()
    {
        ChangeHealth(-damage);

        //canShoot = false;
        //Set thread to turn shooting back on in a short amount of time

        GameObject projectile = Instantiate(projectilePrefab, gameObject.transform.position, gameObject.transform.rotation); //Creates a projectile and removes its health from the player
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
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, boxCollider.size.y / 2 + 0.5f, groundLayer);
        return hit.collider != null;
    }

    //------------------------Health and Damage------------------------
    private void ChangeHealth(float value) //Increases and Decreases player's health
    {
        
        if ((health + value) > MaxHealth) //Health raised to max
            health = MaxHealth;
        else if ((health + value) < 0) //Health lowered to 0
        {
            health = 0;
            Destroy(gameObject);
        }
        else //Raises or lowers health
            health += value;

        for (int i = sizeStages - 1; i >= 0; i--)
        {
            if (health >= (MaxHealth * i / sizeStages))
            {
                float percent = (i + 1.0f) / sizeStages;
                transform.localScale = new Vector3(percent, percent, percent);
                break;
            }
        }
    }

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
            rigidBody.velocity = kbDirection.normalized * kbForce;
            yield return new WaitForSeconds(Time.deltaTime);
            kbLength += Time.deltaTime;
        }
    }

    
}
