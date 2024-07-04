using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenseGuidance : MonoBehaviour
{
    [SerializeField]
    private CharacterController controller;

    [SerializeField]
    private Rigidbody target;

    //ProNav is a powerful alrogrithm. If we want the game fair, we need to introduce some limitations. This buffer serves to introduce reaction time.
    [SerializeField]
    private int reactionTime = 0;
    [SerializeField]
    private bool randomizeReactionTime;
    [SerializeField]
    private int maxReactionTime = 0;
    private Queue<Vector3> reactionBuffer = new(); 

    //Previous values, for computing derivatives.
    private float previousBearing;
    private float previousDeltaBearing;
    private float previousDist;

    public bool RequireLOS = true;

    private void Start()
    {
        InitDefense();
    }

    public void InitDefense()
    {
        if (randomizeReactionTime)
        {
            reactionTime = Random.Range(0, maxReactionTime);
        }
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        ///---ProNav implementation---
        ///The idea here is we want to measure the line of sight between the target and the seeker, and we want to maneuver the seeker to resist change to the direction of that line of sight.
    
        //First order of business, we need to know the direction the target.
        //ProNav doesn't actually require distance information, but it's available and we can use it to improve the algorithm, so we'll preserve it rather than normalizing this vector.
        Vector3 targetDelta = target.position - transform.position;

        //Measure closure rate, because we want to behave differently if we can't close the target
        //We can't lead the target if we can't close the target, but if we just chase the target, maybe we'll get a chance to lead it later.
        float closureRate = targetDelta.magnitude - previousDist;
        previousDist = targetDelta.magnitude;

        //Case where we are able to close the target. Perform ProNav.
        if(closureRate < 0)
        {
            //Compute the line of sight to the target, and measure the rate at which it changes in angle.
            float bearing = Vector3.SignedAngle(Vector3.forward, targetDelta, Vector3.up);
            float deltaBearing = ((bearing - previousBearing) * 100.0f);
            previousBearing = bearing;

            //Also compute the rate of change for the change in bearing: The second derivative. This helps anticipate the player's acceleration as well as velocity.
            float deltaDeltaBearing = deltaBearing - previousDeltaBearing;
            previousDeltaBearing = deltaBearing;

            //Attenuate deltaBearing based on range to target.
            //This isn't strictly necessary, but Smaller inputs create a bigger bearing change at shorter ranges. This makes things a little smoother.
            deltaBearing *= Mathf.Clamp(targetDelta.magnitude / 5, 0, 2);


            //Optionally only perform leading on targets you can verifiably see
            //This isn't super realistic since a real human could make pretty good guesses about where their quarry is, but this abstraction makes the game a little more fair.
            if (RequireLOS)
            {
                RaycastHit hit;
                Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), targetDelta, out hit, targetDelta.magnitude);
                if (hit.collider != null)
                {
                    if (hit.transform.GetComponent<PlayerInputHandler>() != null)
                    {
                        Debug.DrawRay(transform.position + new Vector3(0, 0.5f, 0), targetDelta);

                        //Enqeue the steering command from ProNav into the reaction buffer
                        //We multiply by velocity rather than forward, because we care more about rotating the direction we're headed than the direction we're pointing.
                        reactionBuffer.Enqueue((Quaternion.AngleAxis((deltaBearing + deltaDeltaBearing), Vector3.up) * Vector3.Normalize(GetComponent<Rigidbody>().velocity)));

                    }
                }
            }
            else
            {
                //Enqeue the steering command from ProNav into the reaction buffer regardless of LOS, if desired
                reactionBuffer.Enqueue((Quaternion.AngleAxis((deltaBearing + deltaDeltaBearing), Vector3.up) * Vector3.Normalize(GetComponent<Rigidbody>().velocity)));           
            }
        }

        else
        {
            //Case where we can't close the target: Chase the target instead. This gives us a good chance of getting in range to intercept later.
            reactionBuffer.Enqueue(targetDelta);
        }

        //If there are more entries in the buffer than reaction time, decqueue the first and send its steering command.
        //This writes a steering command to controller that is as many frames old as reactionTime.
        //Framerate is not constant, but since we're simulating perception here, I think this is a reasonable abstraction.
        if(reactionBuffer.Count > reactionTime)
        {
            //Since we're using a Queue, both the reading and writing operations are O(1)
            Vector3 interest = reactionBuffer.Dequeue();
            controller.SetTargetFacing(interest);
        }



        //---Defender Separation---
        ///We're not working in DOTS, so looping over every defender for every defender is not only O(N^2) but also pretty expensive in general because of how GameObjects manage data.
        ///This isn't terrible for a team size of 11, but would scale poorly.
        ///A better solution (DOTS excepted) would be to process all of the defenders together and perform both target leading and separation in a compute shader. That's out-of-scope for this demo,
        ///but an example of my work to this sort of end can be found in this realtime salmon schooling simulation: 
        ///https://drive.google.com/file/d/1uWLlsvrwA3j4ThLFqe2LXy0jj5kDZApG/view?usp=sharing
        ///https://drive.google.com/file/d/1qeSAZvicd_m00AcE05f37QC8yMCMphQd/view?usp=drive_link
        ///This simulation provides for an arbitrary number of fish (performant up to several thousand in count) to avoid each other, align with each other, and pursue goals 
        ///
        ///Since compute shaders are a bit out of scope and probably overkill for a team size of 11, instead we'll tackle the problem by OverlapShereing for nearby defenders and only avoiding them.
        ///OverlapSphere is still pretty expensive compared to similar strategies in DOTS, but it will do in this case.

        Vector3 avoidVec = Vector3.zero;
        Collider[] neighbors = Physics.OverlapSphere(transform.position, 5.0f);

        //For every other defender in a 5 meter radius, attempt to strafe away from that defender by a factor equal to how close you are to it.
        foreach(Collider c in neighbors)
        {
            if(c.gameObject != gameObject && c.GetComponent<DefenseGuidance>() != null)
            {
                Vector3 delta = c.transform.position - transform.position;
                float approachScalar = 5.0f - delta.magnitude;
                avoidVec -= Vector3.Normalize(delta) * 0.1f * approachScalar;
            }
        }

        //Our control command is in local space, but our avoidance vector is in global space. We have to do a quick transform
        avoidVec = transform.InverseTransformVector(avoidVec);

        controller.SetMovement(new Vector3(0, 0, 1.0f) + avoidVec);
    }
}
