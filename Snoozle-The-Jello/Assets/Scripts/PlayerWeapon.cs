using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerWeapon : MonoBehaviour
{
    protected bool attacking;
    protected float speed;
    protected float distThreshold;
    protected Vector3 targetPos;
    protected float damage;
    protected float distance;

    protected Vector3 startPos;
    protected Rigidbody2D rigidBody;

    public float Damage
    {
        get { return damage; }
        set { damage = value; }
    }
    public float Speed
    {
        get { return speed; }
        set { speed = value; }
    }
    public float Distance
    {
        get { return distance; }
        set { distance = value; }
    }

    // Start is called before the first frame update
    protected void Start()
    {
        startPos = transform.localPosition;
        distThreshold = 0.005f;
        rigidBody = GetComponent<Rigidbody2D>();
        rigidBody.bodyType = RigidbodyType2D.Kinematic;
    }

    // Update is called once per frame
    protected void Update()
    {
        if (Vector2.Distance(transform.localPosition, targetPos) > distThreshold) //Weapon moves while not close enough to target
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * speed); 
        else
            targetPos = startPos;


        if (Vector2.Distance(transform.localPosition, targetPos) <= distThreshold && targetPos == startPos) //Weapon is finished moving
        {
            gameObject.SetActive(false);
        }
    }

    public void Attack()
    {
        targetPos = startPos + new Vector3(distance, 0, 0);
    }

    public void Reset() //Resets weapon
    {
        targetPos = startPos;
        transform.localPosition = startPos;
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject other = collision.gameObject;

        if (other.tag == "Enemy")
        {
            other.GetComponent<Character>().TakeDamage(Damage, other.transform.position - transform.position);
            //Reset();
        }
    }
}
