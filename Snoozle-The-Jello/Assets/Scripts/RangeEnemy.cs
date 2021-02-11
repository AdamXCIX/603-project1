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
    [SerializeField] List<float> patrolXPositions;
    private int currentPatrolInd = 0;
    private float prevXPos; 
    private float movementCounter = 0;
    private float shootingCounter = 0;
    [SerializeField] private float shootingLimit; 
    #endregion
    #region Methods
    protected override void Start()
    {
        manager = FindObjectOfType<GameManager>();

        prevXPos = transform.position.x;

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
            shootingCounter = 0; 
        }

        switch (state)
        {
            case EnemyState.Tracking:
                CheckPlayerDirection(); 
                TrackPlayer(); 
                break;
            case EnemyState.Shooting:
                CheckPlayerDirection();

                shootingCounter += Time.deltaTime;

                if (shootingCounter >= shootingLimit)
                {
                    Shoot();
                    shootingCounter = 0;
                }
                break;
            case EnemyState.Patrolling:
                Patrol();
                movementCounter += Mathf.Abs(transform.position.x - prevXPos);
                break;
            case EnemyState.Standing:
                CheckPlayerDirection(); 
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
            Mathf.Abs(manager.player.transform.position.y - transform.position.y) < 0.1f)
        {
            state = EnemyState.Shooting; 
        }
        else if (dist <= playerTrackingRadius && 
            Mathf.Abs(manager.player.transform.position.y - transform.position.y) < 0.1f) 
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
        Vector3 currentPatrolPos = new Vector3(patrolXPositions[currentPatrolInd], transform.position.y, 0);

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
            prevPatrolInd = patrolXPositions.Count - 1; 
        }

        if (dist < 0.1 && movementCounter >= (0.5 * Mathf.Abs(patrolXPositions[currentPatrolInd] - patrolXPositions[prevPatrolInd])))
        {
            currentPatrolInd++; 

            if (currentPatrolInd >= patrolXPositions.Count)
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
    #endregion
}
