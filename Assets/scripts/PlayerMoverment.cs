using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Unity.PlasticSCM.Editor.WebApi;

public class PlayerMoverment : MonoBehaviour
{
    [SerializeField] InputActionReference reset;
    [SerializeField] InputActionReference reverse;
    [SerializeField] Rigidbody rb;
    [SerializeField] List <Transform> wheels;
    [SerializeField] List <Transform> frontWheels;
    [SerializeField] [ExposedScriptableObject] carSettijgs questionable;
    [SerializeField] InputActionReference input;
    [SerializeField] Transform centerOfMass;
    int currentGeaer = 1;
    float forwardInput;
    float currentSteeringAngle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        input.action.Enable();
        reset.action.Enable();
        reverse.action.Enable();
        reset.action.performed += ResetFunc;
        reverse.action.performed += Reverse;
    }
    void OnDisable()
    {
        input.action.Disable();
        reset.action.performed -= ResetFunc;
        reverse.action.performed -= Reverse;
        reverse.action.Disable();
        reset.action.Disable();
    }
    void Start()
    {
        currentSteeringAngle = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 movementOutput = input.action.ReadValue<Vector2>();
        float steeringInput = movementOutput.x;
        forwardInput = movementOutput.y;

        // deadszones
        if (forwardInput < -0.9)
        {
            forwardInput = -1;
        }

        if (forwardInput > -0.1 && forwardInput < 0.1)
        {
            forwardInput = 0;
        }

        // steering
        if (steeringInput > -0.1 && steeringInput < 0.1)
        {
        currentSteeringAngle = Mathf.MoveTowards(currentSteeringAngle, 0, questionable.powerSteering * Time.deltaTime);
        }
        else
        {
            currentSteeringAngle += steeringInput * questionable.powerSteering * Time.deltaTime;
        }
        currentSteeringAngle = Mathf.Clamp(currentSteeringAngle, questionable.minSteeringAngle, questionable.maxSteeringAngle);
        Debug.Log(steeringInput);
        foreach (Transform i in frontWheels)
        {
            i.localRotation = Quaternion.Euler(0, currentSteeringAngle, 0);
        }


    }

    void FixedUpdate()
    {
        foreach (Transform i in wheels)
        {
            RaycastHit hit;
            if (Physics.Raycast(i.position, -i.up, out hit, 
                    questionable.rideHeight + questionable.wheelRadius, questionable.groundMask))
            {
                TireSpringForce(i, hit);
                TireGripForce(i);
                if(forwardInput > 0)
                {
                    accelerate(i);
                }
                if (forwardInput < 0)
                {    
                    braking(i);
                }
            }
        }
        rb.AddForceAtPosition(questionable.gravity*rb.mass*Vector3.down, centerOfMass.position);
    }

    /* void TireGravity(Transform applicableTire)
    {
        rb.AddForceAtPosition(questionable.gravity*rb.mass*Vector3.down, applicableTire.position);
     }
    */
    void TireSpringForce(Transform applicableTire, RaycastHit hit)
    {
        Vector3 springDir = hit.normal;
        Vector3 tireVelocity = rb.GetPointVelocity(applicableTire.position);
        float springLength = questionable.rideHeight - (hit.distance - questionable.wheelRadius);
        float velocityraptor = Vector3.Dot(springDir, tireVelocity);
        float appliedForce = springLength*questionable.springStrength - velocityraptor*questionable.damper;
        rb.AddForceAtPosition(rb.mass/wheels.Count*appliedForce*springDir, applicableTire.position);
    }
    void TireGripForce(Transform applicableTire)
    {
        Vector3 tireGripDirection = applicableTire.right;
        Vector3 tireVelocity = rb.GetPointVelocity(applicableTire.position);
        float effectiveVelocity = Vector3.Dot(tireGripDirection, tireVelocity);

        // grip factor calculation
        float effectiveVelocityPercentage = effectiveVelocity/tireVelocity.magnitude;
        float GripFactor;
        if (frontWheels.Contains(applicableTire))
        {
            GripFactor = questionable.frontGripGraph.Evaluate(effectiveVelocityPercentage);
        }
        else
        {
            GripFactor = questionable.rearGripGraph.Evaluate(effectiveVelocityPercentage);
        }

        float DesiredVelocityChange = -effectiveVelocity*GripFactor;
        float Acceleration = DesiredVelocityChange/Time.fixedDeltaTime;
        rb.AddForceAtPosition(rb.mass/wheels.Count*Acceleration*tireGripDirection, applicableTire.position);
    }

    void accelerate(Transform applicableTire)
    {
        Vector3 accelerationdirection = applicableTire.forward;
        float currentCarSpeed = Vector3.Dot(transform.forward, rb.linearVelocity);
        float normalizedCarSpeed = Mathf.Clamp01(Mathf.Abs(currentCarSpeed)/questionable.gears[currentGeaer].topSpeed);
        float availeableTorque = questionable.gears[currentGeaer].acceleration.Evaluate(normalizedCarSpeed)*questionable.gears[currentGeaer].scale*forwardInput;
        if (currentCarSpeed < questionable.gears[currentGeaer].topSpeed)
        {
            rb.AddForceAtPosition(availeableTorque*(rb.mass/wheels.Count)*accelerationdirection, applicableTire.position);
        }
    }
    void braking(Transform applicableTire)
    {
        Vector3 tireBrakeDirection = applicableTire.forward;
        Vector3 tireVelocity = rb.GetPointVelocity(applicableTire.position);
        float effectiveVelocity = Vector3.Dot(tireBrakeDirection, tireVelocity);

        float effectiveVelocityPercentage = effectiveVelocity/tireVelocity.magnitude;
        float brakeFactor = questionable.braking.Evaluate(effectiveVelocityPercentage);

        float DesiredVelocityChange = effectiveVelocity*brakeFactor*forwardInput;
        float Acceleration = DesiredVelocityChange/Time.fixedDeltaTime;
        rb.AddForceAtPosition(rb.mass/wheels.Count*Acceleration*tireBrakeDirection, applicableTire.position);
    }
    void ResetFunc(InputAction.CallbackContext _)
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }
    void Reverse(InputAction.CallbackContext _)
    {
        if (currentGeaer == 0)
        {
            currentGeaer = 1;
        }
        else
        {
            currentGeaer = 0;
        }
    }
}
