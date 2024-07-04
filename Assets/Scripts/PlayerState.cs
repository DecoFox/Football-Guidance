using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class PlayerState : MonoBehaviour
{
    private float health = 100.0f;
    [SerializeField]
    private Material playerMaterial;

    //UnityEvent for telling the MenuBar instance that a player has been tackled. See MenuBar script comments for details on why this is being done this way.
    [SerializeField]
    private UnityEvent playerTackled;

    void Update()
    {
        //Increment a "tackle" status on the player every tick for each defender they are in contact with
        //This is meant to approximate situations where one or two defenders on the player's tail might be struggling to complete a tackle while not quite managing to fully catch up
        Collider[] neighbors = Physics.OverlapSphere(transform.position, 0.55f);
        bool canRecharge = true;
        foreach (Collider c in neighbors)
        {
            if (c.GetComponent<DefenseGuidance>() != null)
            {
                health -= 1.0f * Time.deltaTime * 100.0f;
                canRecharge = false;
            }
        }

        //Decrement that "tackle" status if the player is free of obstructions
        if (canRecharge)
        {
            health += 1.0f * Time.deltaTime * 100.0f;
        }
        if(health <= 0)
        {
            playerTackled.Invoke();
        }
        health = Mathf.Clamp(health, 0.0f, 100.0f);
        
        playerMaterial.SetFloat("_TackleFactor", 1 - (health / 100));
    }

    public void ResetHealth()
    {
        this.health = 100.0f;
    }

}
