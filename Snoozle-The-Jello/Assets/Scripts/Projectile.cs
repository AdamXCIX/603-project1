using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    #region Fields
    private float damage;

    [SerializeField] Vector2 speed;

    private string origin; 
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
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Speed);

        float viewportPos = Camera.main.WorldToViewportPoint(transform.position).x;

        if (viewportPos < 0 || viewportPos > 1)
            Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" && origin == "Enemy" || collision.gameObject.tag == "Enemy" && origin == "Player")
        {
            collision.gameObject.GetComponent<Character>().TakeDamage(Damage);
            Destroy(gameObject); 
        }
    }
}
