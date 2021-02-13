using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [SerializeField] private float health; //Health restored by pickup

    public float Health //Properties
    {
        get { return health; }
        set { health = value; }
    }
}
