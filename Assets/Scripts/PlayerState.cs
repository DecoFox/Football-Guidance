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
        /*
        Collider[] neighbors = Physics.OverlapSphere(transform.position, 5.0f);
        foreach (Collider c in neighbors)
        {
            if (c.gameObject != gameObject && c.GetComponent<DefenseGuidance>() != null)
            {

            }
        }
        */
        health = Mathf.Clamp(health + 1.0f * Time.deltaTime * 100.0f, 0.0f, 100.0f);
        


        playerMaterial.SetFloat("_TackleFactor", 1 - (health / 100));
    }

    private void OnCollisionStay(Collision collision)
    {
        Collider c = collision.collider;
        if (c.GetComponent<DefenseGuidance>() != null)
        {
            health -= 2.0f;
        }
    }
}
