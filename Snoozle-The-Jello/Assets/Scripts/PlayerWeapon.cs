using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerWeapon : Projectile
{
    protected bool attacking;
    protected float sSpeed;
    protected float distThreshold;
    protected Vector2 targetPos;

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
        rigidBody = GetComponent<Rigidbody2D>();
        rigidBody.bodyType = RigidbodyType2D.Kinematic;
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
