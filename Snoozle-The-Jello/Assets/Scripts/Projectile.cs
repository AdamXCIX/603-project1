using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    #region Fields
    protected float damage;

    [SerializeField] protected Vector2 speed;

    private string origin;

    protected float distance;
    protected Vector2 startPos;
    protected Rigidbody2D rigidBody;
    #endregion

    #region Properties
    public float Damage
    {
        get { return damage; }
        set { damage = value; }
    }
    public Vector2 Speed
    {
        get { return speed; }
        set { speed = value; }
    }
    public string Origin
    {
        get { return origin; }
        set { origin = value; }
    }
    public float Distance
    {
        get { return distance; }
        set { distance = value; }
    }
    #endregion

    // Start is called before the first frame update
    protected virtual void Start()
    {
        startPos = transform.position;
        rigidBody = GetComponent<Rigidbody2D>();
        rigidBody.bodyType = RigidbodyType2D.Kinematic;
        rigidBody.velocity = speed;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (Vector2.Distance(transform.position, startPos) >= distance) //Projectile drops off after hitting max distance
            rigidBody.bodyType = RigidbodyType2D.Dynamic;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject other = collision.gameObject;
        if (other.layer == LayerMask.NameToLayer("Ground")) //Projectile hits ground
            Destroy(gameObject);
        else if (other.tag == "Player" && origin == "Enemy" || other.tag == "Enemy" && origin == "Player")
        {
                other.GetComponent<Character>().TakeDamage(Damage, other.transform.position - transform.position);
            Destroy(gameObject); 
        }
    }
}
