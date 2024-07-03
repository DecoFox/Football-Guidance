using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerState : MonoBehaviour
{
    private float health = 100.0f;
    [SerializeField]
    private Material playerMaterial;

    // Start is called before the first frame update
    void Start()
    {
        
    }

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
        health = Mathf.Clamp(health, 0.0f, 100.0f);
        


        playerMaterial.SetFloat("_TackleFactor", 1 - (health / 100));
    }

}
