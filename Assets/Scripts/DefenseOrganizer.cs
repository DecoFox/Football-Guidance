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

    [SerializeField]
    private MenuBar menu;

    private float speedAverage;
    private float reactionAverage;


    private List<DefenseGuidance> spawnedDefenders = new();

    //Spawn the defenders, optionally destroying and recreating them with new stats
    public void SpawnDefense(bool reinitialize, SpawnContext context)
    {
        if (!reinitialize)
        {
            if(spawnedDefenders.Count == 11)
            {
                foreach(DefenseGuidance g in spawnedDefenders)
                {
                    if(g.GetOrigin() != null)
                    {
                        g.transform.position = g.GetOrigin().position;
                        g.transform.rotation = g.GetOrigin().rotation;
                    }
                    else
                    {
                        print("null origin");
                        //If something goes wrong in the redeploy process, just respawn everything from scratch
                        SpawnDefense(true, context);
                        break;
                    }
                }
            }
            else
            {
                //Ditto
                SpawnDefense(true, context);
            }
        }
        else
        {
            //Zero the speed and reaction average accumulators for the new team comp
            speedAverage = 0;
            reactionAverage = 0;

            //Remove any old defenders
            foreach(DefenseGuidance dg in spawnedDefenders)
            {
                Destroy(dg.gameObject);
            }

            spawnedDefenders.Clear();

            foreach(Transform t in defenseSpawnpoints)
            {
                print("Performing defense initialization...");
                GameObject newDefender = Instantiate(defenderPrefab, t.position, t.rotation);
                CharacterController cc = newDefender.GetComponent<CharacterController>();

                //Set up new CharacterController
                //Default the CC off so they don't start moving right away
                cc.enabled = false;
                if (context.randomSpeed)
                {
                    cc.SetMovementMultiplier(Random.Range(context.minSpeed, context.maxSpeed));
                }
                else
                {
                    cc.SetMovementMultiplier(context.setSpeed);
                }
                cc.InitCharacter();

                DefenseGuidance dg = newDefender.GetComponent<DefenseGuidance>();

                //Set up new DefenseGuidance
                //Default the DG off so they don't start attacking right away
                dg.enabled = false;
                if (context.randomReaction)
                {
                    dg.SetReaction(Random.Range(context.minReaction, context.maxReaction));
                }
                else
                {
                    dg.SetReaction(context.setReaction);
                }

                dg.SetOrigin(t);
                dg.SetTarget(targetPlayer);
                speedAverage += cc.GetMovementMultiplier();
                reactionAverage += dg.GetReaction();
                spawnedDefenders.Add(dg);
            }

            //Update the menu with the final stats of our defending team
            speedAverage /= spawnedDefenders.Count;
            reactionAverage /= spawnedDefenders.Count;

            menu.SetAverages(speedAverage, reactionAverage);

        }
    }

    public void SetDefenseActive(bool setActive)
    {
        foreach(DefenseGuidance dg in spawnedDefenders)
        {
            dg.enabled = setActive;
            dg.GetComponent<CharacterController>().enabled = setActive;
            dg.GetComponent<Rigidbody>().isKinematic = !setActive;
        }
    }

    public struct SpawnContext
    {
        public bool randomSpeed;
        public bool randomReaction;
        public float setSpeed;
        public int setReaction;
        public float minSpeed;
        public float maxSpeed;
        public int minReaction;
        public int maxReaction;
    }


}
