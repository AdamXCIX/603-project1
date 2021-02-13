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
            float ratio = playerScript.Health / playerScript.MHealth; //Gets ratio of player's health to max health

            if (rect.sizeDelta.x != startWidth * ratio) //Updates bar if not up to date
            {
                rect.sizeDelta = new Vector2(startWidth * ratio, rect.sizeDelta.y);

                if (playerScript.Health <= playerScript.Damage) //Changes color if player cannot use 
                    image.color = Color.red;
                else
                    image.color = startColor;
            }

            

        }
    }
}
