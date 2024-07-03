using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController : MonoBehaviour
{

    private GameObject plr;
    private Rigidbody rb;

    private Vector3 targetFacing;
    private Vector3 targetMovement;

    public float movementMultiplier = 1.0f;
    public bool randomizeMovementMultiplier;
    public float randomizeMovementMinimum = 0.75f;
    public float randomizeMovementMaximum = 1.25f;

    
    // Start is called before the first frame update
    void Start()
    {
        plr = gameObject;
        rb = GetComponent<Rigidbody>();
        targetFacing = transform.forward;

        if (randomizeMovementMultiplier)
        {
            movementMultiplier = Random.Range(randomizeMovementMinimum, randomizeMovementMaximum);
        }
    }

    public GameObject GetObject()
    {
        return this.plr;
    }

    public Rigidbody GetRigidbody()
    {
        return this.rb;
    }

    public void SetMovement(Vector3 movementInput)
    {
        this.targetMovement = movementInput;
    }

    public void SetTargetFacing(Vector3 targetFacing)
    {
        this.targetFacing = targetFacing;
    }

    public Vector3 GetTargetFacing()
    {
        return this.targetFacing;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        ///SUMMARY: A simple character controller leveraging the Unity Rigidbody system and direct velocity control.
        ///A system using addForce and considering actual shoe-to-field contact surface area, materials, and relative friction would be much more realistic, but out-of-scope for a demo like this to make feel right.
        ///We have much more direct control over how the player feels this way.
    



        //Transform player input into player-local coordinate system
        Vector3 inputTransform = plr.transform.TransformVector(targetMovement);

        //Add input to the player's velocity
        rb.velocity += new Vector3(inputTransform.x, 0, inputTransform.z) * 0.4f * movementMultiplier;

        //Apply drag from the square of velocity times some number (Shooting for "feels good" here)
        rb.velocity -= rb.velocity * rb.velocity.magnitude * 0.005f;

        //Rotate the player toward their camera direction
        //Favor a torque solution rather than a setting solution because it's a bit more stable for less work.
        float deltaAngle = Vector3.SignedAngle(plr.transform.forward, targetFacing, Vector3.up) / 100.0f;
        rb.AddTorque(new Vector3(0, deltaAngle * 5.0f, 0) + new Vector3(0, rb.angularVelocity.y * -1.0f, 0), 0);

    }

    private void Update()
    {

    }
}
