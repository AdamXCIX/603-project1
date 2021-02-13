using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerWeapon : Projectile
{
    protected bool attacking;
    protected float distance;
    protected float sSpeed;
    protected float distThreshold;
    protected Vector2 startPos;
    protected Vector2 targetPos;
    protected Rigidbody2D rigidbody2D;

    public float Distance
    {
        get { return distance; }
        set { distance = value; }
    }

    public float SSpeed
    {
        get { return sSpeed; }
        set { sSpeed = value; }
    }


    // Start is called before the first frame update
    protected override void Start()
    {
        startPos = transform.localPosition;
        distThreshold = 0.005f;
        rigidbody2D = GetComponent<Rigidbody2D>();
        rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (Vector2.Distance(transform.localPosition, targetPos) > distThreshold) //Weapon moves while not close enough to target
            transform.localPosition = Vector2.Lerp(transform.localPosition, targetPos, Time.deltaTime * sSpeed); 
        else
            targetPos = startPos;


        if (Vector2.Distance(transform.localPosition, targetPos) <= distThreshold && targetPos == startPos) //Weapon is finished moving
        {
            gameObject.SetActive(false);
        }
    }

    public void Attack()
    {
        targetPos = startPos + new Vector2(distance, 0);
    }
}
