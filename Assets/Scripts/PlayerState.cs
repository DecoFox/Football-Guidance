using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class PlayerState : MonoBehaviour
{
    private float health = 100.0f;
    [SerializeField]
    private Material playerMaterial;

    //Working with prefabs etc and in multiple scenes, it would in most cases be favorable to rely on C# events and an event manager, but this is expedient for a single-scene demo
    [SerializeField]
    private UnityEvent playerTackled;

    // Update is called once per frame
    void Update()
    {
        
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
