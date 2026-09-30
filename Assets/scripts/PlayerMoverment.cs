using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerMoverment : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] List <Transform> wheels;
    [SerializeField] List <Transform> frontWheels;
    [SerializeField] [ExposedScriptableObject] carSettijgs questionable;
    [SerializeField] InputActionReference input;
    [SerializeField] Transform centerOfMass;
    float forwardInput;
    float currentSteeringAngle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentSteeringAngle = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 movementOutput = input.action.ReadValue<Vector2>();
        float steeringInput = movementOutput.x;
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
        rb.AddForceAtPosition(appliedForce*springDir*rb.mass/wheels.Count, applicableTire.position);
    }
    
    void OnEnable()
    {
        input.action.Enable();
    }
    void OnDisable()
    {
        input.action.Disable();
    }
    void TireGripForce(Transform applicableTire)
    {
        Vector3 tireGripDirection = applicableTire.right;
        Vector3 tireGripDirectionVelocity = rb.GetPointVelocity(applicableTire.position);
        float theActualTireGripDirectionVelocity = Vector3.Dot(tireGripDirection, tireGripDirectionVelocity);

        // grip factor calculation
        float theActualTireGripDirectionVelocityPercentage = theActualTireGripDirectionVelocity/tireGripDirectionVelocity.magnitude;
        float theActualTireGripDirectionVelocityPercentageGripFactor;
        if (frontWheels.Contains(applicableTire))
        {
            theActualTireGripDirectionVelocityPercentageGripFactor = questionable.frontGripGraph.Evaluate(theActualTireGripDirectionVelocityPercentage);
        }
        else
        {
            theActualTireGripDirectionVelocityPercentageGripFactor = questionable.rearGripGraph.Evaluate(theActualTireGripDirectionVelocityPercentage);
        }

        float theActualTireGripDirectionVelocityPercentageDesiredVelocityChange = -theActualTireGripDirectionVelocity*theActualTireGripDirectionVelocityPercentageGripFactor;
        float theActualTireGripDirectionVelocityPercentageDesiredVelocityChangeAcceleration = theActualTireGripDirectionVelocityPercentageDesiredVelocityChange/Time.fixedDeltaTime;
        rb.AddForceAtPosition(tireGripDirection*theActualTireGripDirectionVelocityPercentageDesiredVelocityChangeAcceleration*rb.mass/wheels.Count, applicableTire.position);
    }
}
