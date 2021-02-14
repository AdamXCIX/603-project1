using System.Collections;
using System.Collections.Generic;
using UnityEngine;
enum EnemyState
{
    Tracking, 
    Shooting, 
    Patrolling, 
    Standing
}

public class RangeEnemy : Character
{
    #region Fields
    private EnemyState state;
    private EnemyState prevState;

    [SerializeField] private GameManager manager;
    [SerializeField] private float playerTrackingRadius;
    [SerializeField] private float playerShootRadius;
    [SerializeField] private List<GameObject> PatrolPositions = new List<GameObject>(2);
    [SerializeField] private float shotSpeed; //Projectile Speed
    [SerializeField] private float shotDistance; //Distance Projectile moves before dropping
    [SerializeField] private float shotDelay; //econds between Projectile shots

    private int currentPatrolInd = 0;
    private float prevXPos; 
    private float movementCounter = 0;
    private bool canShoot; 
    #endregion

    #region Methods
    protected override void Start()
    {
        manager = FindObjectOfType<GameManager>();

        prevXPos = transform.position.x;

        canShoot = true;

        base.Start();
    }

    protected override void Update()
    {
        Debug.DrawLine(transform.position, transform.position + transform.right, Color.red);
        Debug.DrawLine(transform.position, transform.position + transform.up, Color.blue);

        CheckPlayerPos();

        if (prevState != state)
        {
            movementCounter = 0;
        }

        switch (state)
        {
            case EnemyState.Tracking:
                CheckPlayerDirection(); 
                TrackPlayer(); 
                break;
            case EnemyState.Shooting:
                CheckPlayerDirection();

                if (canShoot)
                {
                    GetComponent<Animator>().SetTrigger("Shooting");
                    canShoot = false;
                }
                break;
            case EnemyState.Patrolling:
                Patrol();
                movementCounter += Mathf.Abs(transform.position.x - prevXPos);
                break;
            case EnemyState.Standing:
                CheckPlayerDirection();
                GetComponent<Animator>().SetInteger("AnimState", 0);
                break;
        }

        prevState = state; //Updates previous state
        prevXPos = transform.position.x;
    }

    /// <summary>
    /// Helper method to return the distance this enemy is from the player
    /// </summary>
    /// <returns>Returns a float indicating the distance from the player</returns>
    private float DistanceFromPlayer()
    {
        Vector3 playerPos = manager.player.transform.position;

        return Vector3.Distance(playerPos, transform.position); 
    }

    /// <summary>
    /// Checks this enemy's distance from the player and updates their state accordingly
    /// </summary>
    private void CheckPlayerPos()
    {
        float dist = DistanceFromPlayer();
        if (dist <= playerShootRadius &&
            Mathf.Abs(manager.player.transform.position.y - transform.position.y) < 0.3f)
        {
            if (state != EnemyState.Standing)
            {
                state = EnemyState.Standing;
            }
            else
                state = EnemyState.Shooting; 
        }
        else if (dist <= playerTrackingRadius && 
            Mathf.Abs(manager.player.transform.position.y - transform.position.y) < 0.3f) 
        {
            state = EnemyState.Tracking;
        }
        else
        {
            state = EnemyState.Patrolling; 
        }
    }

    /// <summary>
    /// Checks the direction the player is from this enemy and updates their sprite if needed
    /// </summary>
    private void CheckPlayerDirection()
    {
        float dotResult = Vector2.Dot(transform.right, manager.player.transform.position - transform.position);

        Debug.Log(dotResult);

        if (dotResult < 0 && !spriteRenderer.flipX)
        {
            spriteRenderer.flipX = true;
        }
        else if (dotResult >= 0 && spriteRenderer.flipX)
        {
            spriteRenderer.flipX = false;
        }
    }

    /// <summary>
    /// Makes this enemy patrol between their set of patrol points
    /// </summary>
    private void Patrol()
    {
        Vector3 currentPatrolPos = PatrolPositions[currentPatrolInd].transform.position;

        float dotResult = Vector2.Dot(transform.right, currentPatrolPos - transform.position);

        if (dotResult < 0)
        {
            Move(-walkSpeed);
            spriteRenderer.flipX = true;
        }
        else if (dotResult >= 0)
        {
            Move(walkSpeed);
            spriteRenderer.flipX = false;
        }

        float dist = Vector3.Distance(transform.position, currentPatrolPos);

        int prevPatrolInd = currentPatrolInd - 1; 
        if (prevPatrolInd < 0)
        {
            prevPatrolInd = PatrolPositions.Count - 1; 
        }

        if (dist < 0.1 && movementCounter >= (0.5 * Mathf.Abs(PatrolPositions[currentPatrolInd].transform.position.x - PatrolPositions[prevPatrolInd].transform.position.x)))
        {
            currentPatrolInd++; 

            if (currentPatrolInd >= PatrolPositions.Count)
            {
                currentPatrolInd = 0; 
            }

            movementCounter = 0; 
        }
    }

    /// <summary>
    /// Makes this enemy track the player once they are close enough
    /// </summary>
    private void TrackPlayer()
    {
        float dotResult = Vector2.Dot(transform.right, manager.player.transform.position - transform.position);

        if (dotResult < 0)
        {
            Move(-walkSpeed);
            spriteRenderer.flipX = true;
        }
        else if (dotResult >= 0)
        {
            Move(walkSpeed);
            spriteRenderer.flipX = false;
        }
    }

    protected override void Shoot() //Ranged Attack
    {
        Vector3 pos = gameObject.transform.position;
        pos.y -= 0.1f * transform.localScale.y;
        pos.z = 1;

        GameObject projectile = Instantiate(projectilePrefab, pos, gameObject.transform.rotation); //Creates a projectile and removes its health from the player
        projectile.transform.localScale = transform.localScale;
        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript)
        {
            projScript.Damage = damage; //Set shot damage
            projScript.Distance = shotDistance; //Set shot damage
            if (!spriteRenderer.flipX) //Set shot direction
                projScript.Speed = shotSpeed * Vector2.right;
            else
                projScript.Speed = shotSpeed * Vector2.left;

            projScript.Origin = "Enemy";
        }

        //Set thread to turn shooting back on in a short amount of time
        StartCoroutine(DelayNextShot(shotDelay));
    }

    IEnumerator DelayNextShot(float shotDelay)
    {
        float shotLength = 0; //Time since last shot

        while (shotLength < shotDelay)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            shotLength += Time.deltaTime;
        }

        canShoot = true;
    }

    protected override void Move(float speed)
    {
        GetComponent<Animator>().SetInteger("AnimState", 1);
        base.Move(speed);
    }
    #endregion
}
