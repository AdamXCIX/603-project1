using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    RectTransform rect;
    Image image;

    float startWidth;
    Color startColor;

    GameObject player;
    Player playerScript;

    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        startWidth = rect.sizeDelta.x;
        startColor = image.color;

        player = GameObject.Find("Player");
        if (player)
            playerScript = player.GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerScript)
        {
            rect.localScale = new Vector3(
                playerScript.Health / playerScript.MHealth,
                1,
                1
            );

            if (playerScript.Health <= playerScript.Damage) //Changes color if player cannot use 
                image.color = Color.red;
            else
                image.color = startColor;
        }
    }
}
