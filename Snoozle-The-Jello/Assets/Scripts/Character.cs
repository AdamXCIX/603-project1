﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    #region Fields
    protected float health;

    [SerializeField] protected float damage;

    [SerializeField] protected GameObject projectilePrefab;

    [SerializeField] protected float MaxHealth; 
    #endregion

    #region Methods
    // Start is called before the first frame update
    protected virtual void Start()
    {
        health = MaxHealth; 
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

    protected virtual void Shoot()
    {
        TakeDamage(damage);

        GameObject projectile = Instantiate(projectilePrefab);
        projectile.GetComponent<Projectile>().Damage = damage;
        projectile.GetComponent<Projectile>().Speed = new Vector2(1, 0); 
    }

    public virtual void TakeDamage(float damage)
    {
        health -= damage;

        if (health >= ((MaxHealth * 2) / 3))
        {
            transform.localScale = new Vector3(1,1,1); 
        }
        else if (health < ((MaxHealth * 2) / 3) && health > (MaxHealth / 3))
        {
            transform.localScale = new Vector3(0.67f, 0.67f, 0.67f);
        }
        else
        {
            transform.localScale = new Vector3(0.33f, 0.33f, 0.33f);
        }
    }

    public virtual void Move()
    {

    }
    #endregion
}