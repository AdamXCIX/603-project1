using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectile : Projectile
{
    protected float distance;
    private Vector2 startPos;
    Rigidbody2D rigidbody2D;

    public float Distance
    {
        get { return distance; }
        set { distance = value; }
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        startPos = transform.position;
        rigidbody2D = GetComponent<Rigidbody2D>();
        rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        rigidbody2D.velocity = speed;
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (Vector2.Distance(transform.position, startPos) >= distance) //Projectile drops off after hitting max distance
            rigidbody2D.bodyType = RigidbodyType2D.Dynamic; 
    }

    private void OnTriggerEnter2D(Collider2D collision) //Destroys projectile when hitting ground
    {
        GameObject other = collision.gameObject;
        if (other.layer == LayerMask.NameToLayer("Ground")) //Projectile hits ground
            Destroy(gameObject);
    }
}
