using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    #region Fields
    protected float health;

    [SerializeField] protected float damage;

    [SerializeField] protected GameObject projectilePrefab;

    [SerializeField] protected float MaxHealth;
    [SerializeField] protected float kbForce; //Knockback force

    protected SpriteRenderer spriteRenderer;
    protected Rigidbody2D rigidBody;
    protected BoxCollider2D boxCollider2D;

    [SerializeField] protected float walkSpeed;


    public float Damage
    {
        get { return damage; }
    }

    #endregion

    #region Methods
    // Start is called before the first frame update
    protected virtual void Start()
    {
        health = MaxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();

        rigidBody.freezeRotation = true; //Prevents player from rotating
    }

    // Update is called once per frame
    protected virtual void Update()
    {

    }

    protected virtual void Shoot()
    {
        TakeDamage(damage, new Vector2());

        Vector3 projectilePos;

        if (!spriteRenderer.flipX)
            projectilePos = transform.position + new Vector3(0.1f, 0, 0);
        else
            projectilePos = transform.position + new Vector3(-0.1f, 0, 0); 

        GameObject projectile = Instantiate(projectilePrefab, projectilePos, Quaternion.identity);
        projectile.GetComponent<Projectile>().Damage = damage;
        projectile.GetComponent<Projectile>().Speed = new Vector2(0.015f, 0);
        projectile.GetComponent<Projectile>().Origin = tag;
    }

    public virtual void TakeDamage(float damage, Vector2 kbDirection)
    {
        health -= damage;

        if (health >= ((MaxHealth * 2) / 3))
        {
            transform.localScale = new Vector3(1,1,1); 
        }
        else if (health < ((MaxHealth * 2) / 3) && health > (MaxHealth / 3))
        {
            transform.localScale = new Vector3(0.67f, 0.67f, 1.0f);
        }
        else
        {
            transform.localScale = new Vector3(0.33f, 0.33f, 1.0f);
        }

        StartCoroutine(TakeKnockBack(0.1f, kbDirection));
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
    }


    protected virtual void Move(float speed)
    {
        rigidBody.velocity = new Vector2(speed, rigidBody.velocity.y);
    }
    #endregion
}
