using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenseOrganizer : MonoBehaviour
{
    [SerializeField]
    private List<Transform> defenseSpawnpoints;

    [SerializeField]
    private GameObject defenderPrefab;

    [SerializeField]
    private Rigidbody targetPlayer;

    private List<DefenseGuidance> spawnedDefenders = new();
    // Start is called before the first frame update
    void Start()
    {
        SpawnDefense(true);
    }

    public void SpawnDefense(bool reinitialize)
    {
        if (!reinitialize)
        {
            if(spawnedDefenders.Count == 11)
            {
                foreach(DefenseGuidance g in spawnedDefenders)
                {
                    if(g.originTransform != null)
                    {
                        g.transform.position = g.originTransform.position;
                        g.transform.rotation = g.originTransform.rotation;
                    }
                    else
                    {
                        //If something goes wrong in the redeploy process, just respawn everything from scratch
                        SpawnDefense(false);
                    }
                }
            }
            else
            {
                //Ditto
                SpawnDefense(false);
            }
        }
        else
        {
            foreach(Transform t in defenseSpawnpoints)
            {
                print("Performing defense initialization...");
                GameObject newDefender = Instantiate(defenderPrefab, t.position, t.rotation);
                CharacterController cc = newDefender.GetComponent<CharacterController>();
                cc.InitCharacter();
                DefenseGuidance dg = newDefender.GetComponent<DefenseGuidance>();
                dg.InitDefense();
                dg.SetTarget(targetPlayer);
                spawnedDefenders.Add(dg);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
