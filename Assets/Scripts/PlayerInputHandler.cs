using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    private Control control;
    private float cameraYawAccumulator = 0;
    private float cameraHeightAccumulator = 1.25f;


    [SerializeField]
    private GameObject playerCamera;

    [SerializeField]
    private CharacterController controller;

    private bool cameraInit = false;

    // Start is called before the first frame update
    void Start()
    {
        control = new Control();
        control.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        //Control input gathering. It would be more efficient to do this with callbacks, but for this application the differences are marginal
        //Obtain player input as a Vector. Vector3 so we don't have to mess with y vs z later.
        Vector3 inputVector = new Vector3(control.Base.Movement.ReadValue<Vector2>().x, 0, control.Base.Movement.ReadValue<Vector2>().y);

        //Obtain player mouse input for camera control
        Vector2 mouseInput = control.Base.CameraRotation.ReadValue<Vector2>();
        if (cameraInit)
        {
            cameraYawAccumulator += mouseInput.x / 100.0f;
            cameraHeightAccumulator += mouseInput.y / -100.0f;
        }
        cameraHeightAccumulator = Mathf.Clamp(cameraHeightAccumulator, -0.5f, 3.0f);
        cameraYawAccumulator %= 6.28f;

        //Forward control input to the movement script
        //These are separated because we would like to use the same movement behavior for players and NPCs
        controller.SetMovement(inputVector);

        if (playerCamera != null)
        {
            float cameraDistance = 4;

            //A little bit of running camera shake, for fun.
            //We sample perlin at the player's position and bump the camera along the perlin
            //We do the same thing for camera roll, but at an offset so the bumping and rolling do not appear coupled.
            Transform plrTransform = controller.GetObject().transform;
            Rigidbody plrRB = controller.GetRigidbody();
            float cameraShake = ((Mathf.PerlinNoise(plrTransform.position.x, plrTransform.position.z) - 0.5f) / 36.0f) * plrRB.velocity.magnitude;
            float cameraTumble = ((Mathf.PerlinNoise(plrTransform.position.x + 1500, plrTransform.position.z + 1500) - 0.5f) / 5.0f) * plrRB.velocity.magnitude;

            //Rotate the camera around the player with some basic trig, and point it at a position some fraction of the player's velocity in front of the player.
            playerCamera.transform.position = plrTransform.position + new Vector3(Mathf.Sin(cameraYawAccumulator) * cameraDistance, cameraHeightAccumulator + cameraShake, Mathf.Cos(cameraYawAccumulator) * cameraDistance);
            playerCamera.transform.LookAt(plrTransform.position + new Vector3(0, 1, 0) + (plrRB.velocity / 10.0f));

            //Apply the camera shake.
            playerCamera.transform.Rotate(Vector3.forward, cameraTumble, Space.Self);

            //Apply the current camera direction as our desired rotation input.
            controller.SetTargetFacing(playerCamera.transform.forward);
            cameraInit = true;
        }
    }

    public void ResetCamera()
    {
        this.cameraHeightAccumulator = 1.25f;
        this.cameraYawAccumulator = 0.0f;
    }
}
