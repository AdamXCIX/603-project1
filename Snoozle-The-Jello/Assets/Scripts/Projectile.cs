using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    #region Fields
    protected float damage;

    protected Vector2 speed; 
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
    #endregion

    // Start is called before the first frame update
    protected virtual void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        transform.Translate(Speed); 
    }
}
