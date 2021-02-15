using System.Collections;
using System.Collections.Generic;
using UnityEngine;
enum PlayerState
{
    Stand,
    Walk,
    Jump,
    Shoot,
    Swipe
}
public class Player : Character
{
    //For spritesheet animations
    //http://www.strandedsoft.com/using-spritesheets-with-unity3d/

    [SerializeField] private float jumpSpeed; //Initial Jump Speed
    [SerializeField] private float shotSpeed; //Projectile Speed
    [SerializeField] private float shotDistance; //Distance Projectile moves before dropping
    [SerializeField] private float shotDelay; //Seconds between Projectile shots
    [SerializeField] private float swipeSpeed; //Projectile Speed
    [SerializeField] private float swipeDistance; //Distance Projectile moves before returning
    [SerializeField] private float swipeDelay; //Seconds between Projectile shots
    //[SerializeField] private float kbForce; //Knockback force
    [SerializeField] private float regenDelay; //Seconds between health regenerations
    [SerializeField] private int sizeStages; //Number of stages for size changes
    [SerializeField] private GameObject weapon; //Reference to weapon
    

    private bool canJump; //Player can jump
    private bool onGround; //Player is on ground
    private bool canShoot; //Player can shoot
    private bool canSwipe; //Player can swipe attack
    //private bool canTakeDamage; //Player can be damaged
    private bool isDead;

    private float timeSinceRegen;


    private Animator animator;
    private BoxCollider2D boxCollider;
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

    // Start is called before the first frame update
    protected override void Start()
    {
        canShoot = true;
        canSwipe = true;
        canTakeDamage = true;

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        groundLayer = LayerMask.GetMask("Ground");
        state = PlayerState.Stand;
        prevState = state;

        weapon.GetComponent<PlayerWeapon>().Speed = swipeSpeed;
        weapon.GetComponent<PlayerWeapon>().Damage = damage;
        weapon.GetComponent<PlayerWeapon>().Distance = swipeDistance;
        weapon.SetActive(false);
        rigidBody.freezeRotation = true; //Prevents player from rotating

        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (!isDead)
        {
            onGround = CheckIfOnGround(); //Check whether player is on ground

            switch (state)
            {
                //Player is not moving
                case PlayerState.Stand:
                    //animator.Play("PlayerIdle");
                    if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) ||
                        Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) //Player Walks
                        state = PlayerState.Walk;
                    else if (Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.Space)) //Player Jumps
                    {
                        canJump = true;
                        state = PlayerState.Jump;
                    }
                    else if (Input.GetKey(KeyCode.J)) //Ranged Attack
                        state = PlayerState.Shoot;
                    else if (Input.GetKey(KeyCode.L)) //Melee attack
                        state = PlayerState.Swipe;
                    break;

                //Player is moving
                case PlayerState.Walk:
                    //animator.Play("PlayerWalk");
                    if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) //Player Moves Left
                        Move(-walkSpeed);
                    else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) //Player Moves Right
                        Move(walkSpeed);
                    else
                        state = PlayerState.Stand;

                    if (Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.Space)) //Player Jumps
                    {
                        canJump = true;
                        state = PlayerState.Jump;
                    }
                    else if (Input.GetKeyDown(KeyCode.J)) //Ranged Attack
                        state = PlayerState.Shoot;
                    else if (Input.GetKeyDown(KeyCode.L)) //Melee attack
                        state = PlayerState.Swipe;
                        

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

                    if (Input.GetKey(KeyCode.J)) //Ranged Attack
                        state = PlayerState.Shoot;
                    else if (Input.GetKey(KeyCode.L)) //Melee attack
                        state = PlayerState.Swipe;
                    else
                        state = PlayerState.Stand;
                    break;

                //Player is using ranged attack
                case PlayerState.Shoot:

                    if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) //Player Moves Left
                        Move(-walkSpeed);
                    else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) //Player Moves Right
                        Move(walkSpeed);

                    if (canShoot && (health > damage)) //Shoots if player is able
                    {
                        Shoot();
                        animator.Play("PlayerShoot", -1, 0f);
                    }

                    if (Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.Space)) //Player Jumps
                    {
                        canJump = true;
                        state = PlayerState.Jump;
                    }
                    else if (!Input.GetKey(KeyCode.J)) //Ends state once button is released
                        state = PlayerState.Stand;

                    break;

                //Player is using melee attack
                case PlayerState.Swipe:
                    if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) //Player Moves Left
                        Move(-walkSpeed);
                    else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) //Player Moves Right
                        Move(walkSpeed);

                    if (canSwipe) //Swipes if player is able
                    {
                        Swipe();
                        animator.Play("PlayerSwipe", -1, 0f);
                    }

                    if (Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.Space)) //Player Jumps
                    {
                        canJump = true;
                        state = PlayerState.Jump;
                    }
                    else if (!Input.GetKey(KeyCode.L)) //Ends state once animation is over
                        state = PlayerState.Stand;

                    break;
            }

            if (prevState != state &&
                ((!animator.GetCurrentAnimatorStateInfo(0).IsName("PlayerShoot") && !animator.GetCurrentAnimatorStateInfo(0).IsName("PlayerSwipe")) ||
                (animator.GetCurrentAnimatorStateInfo(0).IsName("PlayerShoot") || animator.GetCurrentAnimatorStateInfo(0).IsName("PlayerSwipe")) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f))
            {
                if (state == PlayerState.Stand)
                    animator.Play("PlayerIdle", -1, 0f);
                else if (state == PlayerState.Walk)
                    animator.Play("PlayerWalk", -1, 0f);

            }

            prevState = state; //Updates previous state
            
            if (health != MaxHealth)
            {
                RegenerateHealth();
            }
        }
    }

    //------------------------Basic Controls------------------------
    protected override void Move(float speed) //Moves the player horizontally
    {
        rigidBody.velocity = new Vector2(speed, rigidBody.velocity.y);
        if (speed > 0) //Player Faces Left
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (speed < 0) //Player Faces Left
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * -1, transform.localScale.y, transform.localScale.z);
    }

    private void Jump(float speed) //Allows the player to jump
    {
        //rigidbody2D.AddForce(new Vector2(0, force), ForceMode2D.Impulse);
        rigidBody.velocity = new Vector2(rigidBody.velocity.x, speed);
    }

    protected override void Shoot() //Ranged Attack
    {
        ChangeHealth(-damage);   

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
            projScript.Origin = tag;
            if (transform.localScale.x > 0) //Set shot direction
                projScript.Speed = shotSpeed * Vector2.right;
            else
                projScript.Speed = shotSpeed * Vector2.left;
        }

        //Set thread to turn shooting back on in a short amount of time
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

    protected void Swipe() //Melee Attack
    {
        weapon.SetActive(true);
        PlayerWeapon wpnScript = weapon.GetComponent<PlayerWeapon>();

        if (wpnScript)
        if (wpnScript)
        {
            wpnScript.Attack();
        }

        //Set thread to turn swiping back on in a short amount of time
        StartCoroutine(DelayNextSwipe(swipeDelay));
    }

    IEnumerator DelayNextSwipe(float shotDelay)
    {
        canSwipe = false;
        float swipeLength = 0; //Time since last swipe

        while (swipeLength < shotDelay)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            swipeLength += Time.deltaTime;
        }
        canSwipe = true;
    }




    //------------------------Collision Handling------------------------
    private void OnCollisionEnter2D(Collision2D collision) //Handles collisions between player and physical GameObjects
    {
        GameObject other = collision.gameObject;
        Vector2 collisionDirection = transform.position - other.transform.position;

        if (canTakeDamage) //Player touches an enemy
        {
            if (other.tag == "Enemy") //Player takes damage from contact with enemy
            {
                RangeEnemy rangeScript = other.GetComponent<RangeEnemy>();
                MeleeEnemy meleeScript = other.GetComponent<MeleeEnemy>();
                if (rangeScript) 
                    TakeDamage(rangeScript.Damage, collisionDirection);
                else if (meleeScript)
                    TakeDamage(meleeScript.Damage, collisionDirection);
            }
            else if (other.tag == "EnemyProjectile") //Player touches an enemy's projectile
            {
                Projectile projScript = other.GetComponent<Projectile>();
                if (projScript) //Player takes damage from contact with enemy's projectile
                {
                    TakeDamage(projScript.Damage, collisionDirection);
                    Destroy(other);
                }
            }
        }
        
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
        Vector2 leftPos = transform.position - new Vector3(boxCollider.size.x / 2, 0, 0);
        Vector2 rightPos = transform.position + new Vector3(boxCollider.size.x / 2, 0, 0);

        RaycastHit2D leftHit = Physics2D.Raycast(leftPos, Vector2.down, boxCollider.size.y / 2 + 0.5f, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(rightPos, Vector2.down, boxCollider.size.y / 2 + 0.5f, groundLayer);
        return (leftHit.collider != null || rightHit.collider != null);
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

        if (health > damage) //Changes color to show whether player can shoot
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
                if (transform.localScale.x > 0) //Set player direction
                    transform.localScale = new Vector3(percent, percent, 1);
                else
                    transform.localScale = new Vector3(-percent, percent, 1);
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
        }
        else //Increment time
        {
            timeSinceRegen += Time.deltaTime;
        }
    }

    public override void TakeDamage(float value, Vector2 kbDirection) //Decreases player's health and Handles knockback
    {
        ChangeHealth(-value);
        
        if (health > 0)
        {
            StartCoroutine(Flash(1.0f, 0.05f));
            StartCoroutine(TakeKnockBack(0.1f, kbDirection));
        }
    }

    //------------------------Damage Indicators------------------------
    /*protected IEnumerator Flash(float flashDuration, float flashDelay)
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

    protected IEnumerator TakeKnockBack(float kbDuration, Vector2 kbDirection)
    {
        float kbLength = 0; //Time player has been flashing

        while (kbLength < kbDuration)
        {
            rigidBody.velocity = kbDirection.normalized * kbForce;
            yield return new WaitForSeconds(Time.deltaTime);
            kbLength += Time.deltaTime;
        }
    }*/
}
