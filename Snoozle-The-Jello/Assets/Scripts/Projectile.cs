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
    private Vector2 startPos;
    Rigidbody2D rigidbody2D;
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
        rigidbody2D = GetComponent<Rigidbody2D>();
        rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        rigidbody2D.velocity = speed;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (Vector2.Distance(transform.position, startPos) >= distance) //Projectile drops off after hitting max distance
            rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && origin == "Enemy" || collision.gameObject.tag == "Enemy" && origin == "Player")
        {
            collision.gameObject.GetComponent<Character>().TakeDamage(Damage);
            Destroy(gameObject); 
        }
    }
}
