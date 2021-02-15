using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectile : Projectile
{
    // Update is called once per frame
    protected override void Update()
    {
        if (Vector2.Distance(transform.position, startPos) >= distance) //Projectile drops off after hitting max distance
            rigidBody.bodyType = RigidbodyType2D.Dynamic; 
    }
}
