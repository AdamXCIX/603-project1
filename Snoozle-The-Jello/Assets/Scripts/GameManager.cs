using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Fields
    public GameObject player;
    public Dictionary<string, List<GameObject>> enemies = new Dictionary<string, List<GameObject>>();
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        enemies.Add("Ranged", new List<GameObject>());
        enemies.Add("Melee", new List<GameObject>()); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
