using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Control control;
    private GameObject plr;
    private Rigidbody rb;

    private float cameraYawAccumulator = 0;
    private float cameraHeightAccumulator = 1.25f;


    [SerializeField]
    private GameObject camera;

    
    // Start is called before the first frame update
    void Start()
    {
        control = new Control();
        control.Enable();
        plr = gameObject;
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        ///SUMMARY: A simple character controller leveraging the Unity Rigidbody system and direct velocity control.
        ///A system using addForce and considering actual shoe-to-field contact surface area, materials, and relative friction would be much more realistic, but out-of-scope for a demo like this to make feel right.
        ///We have much more direct control over how the player feels this way.
    
        //Control input gathering. It would be more efficient to do this with callbacks, but for this application the differences are marginal
        //Obtain player input as a Vector. Vector3 so we don't have to mess with y vs z later.
        Vector3 inputVector = new Vector3(control.Base.Movement.ReadValue<Vector2>().x, 0, control.Base.Movement.ReadValue<Vector2>().y) * 0.4f;

        //Obtain player mouse input for camera control
        Vector2 mouseInput = control.Base.CameraRotation.ReadValue<Vector2>();
        cameraYawAccumulator += mouseInput.x / 100.0f;
        cameraHeightAccumulator += mouseInput.y / -100.0f;
        cameraHeightAccumulator = Mathf.Clamp(cameraHeightAccumulator, -0.5f, 3.0f);
        cameraYawAccumulator %= 6.28f;


        //Transform player input into player-local coordinate system
        Vector3 inputTransform = plr.transform.TransformVector(inputVector);

        //Add input to the player's velocity
        rb.velocity += new Vector3(inputTransform.x, 0, inputTransform.z);

        //Apply drag from the square of velocity times some number (Shooting for "feels good" here)
        rb.velocity -= rb.velocity * rb.velocity.magnitude * 0.005f;

        //Rotate the player toward their camera direction
        //Favor a torque solution rather than a setting solution because it's a bit more stable for less work.
        float deltaAngle = Vector3.SignedAngle(plr.transform.forward, camera.transform.forward, Vector3.up) / 100.0f;
        rb.AddTorque(new Vector3(0, deltaAngle * 5.0f, 0) + new Vector3(0, rb.angularVelocity.y * -1.0f, 0), 0);

    }

    private void Update()
    {
        float cameraDistance = 4;

        //A little bit of running camera shake, for fun.
        //We sample perlin at the player's position and bump the camera along the perlin
        //We do the same thing for camera roll, but at an offset so the bumping and rolling do not appear coupled.
        float cameraShake = ((Mathf.PerlinNoise(plr.transform.position.x, plr.transform.position.z) - 0.5f) / 36.0f) * rb.velocity.magnitude;
        float cameraTumble = ((Mathf.PerlinNoise(plr.transform.position.x + 1500, plr.transform.position.z + 1500) - 0.5f) / 5.0f) * rb.velocity.magnitude;

        camera.transform.position = plr.transform.position + new Vector3(Mathf.Sin(cameraYawAccumulator) * cameraDistance, cameraHeightAccumulator + cameraShake, Mathf.Cos(cameraYawAccumulator) * cameraDistance);
        camera.transform.LookAt(plr.transform.position + new Vector3(0, 1, 0) + (rb.velocity / 10.0f));
        camera.transform.Rotate(Vector3.forward, cameraTumble, Space.Self);
    }
}
