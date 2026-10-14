using UnityEngine;
using System.Collections.Generic;
using System;

public class Movement : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] List <Transform> wheels;
    [SerializeField] List <Transform> frontWheels;
    [SerializeField] [ExposedScriptableObject] carSettings carData;
    [SerializeField] Transform centerOfMass;
    [NonSerialized] public int currentGear = 1;
    [NonSerialized] public float forwardInput;
    [NonSerialized] public float steeringInput;
    float currentSteeringAngle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentSteeringAngle = 0;
    }
    protected virtual void Update()
    {

        // steering
        if (steeringInput > -0.1 && steeringInput < 0.1)
        {
        currentSteeringAngle = Mathf.MoveTowards(currentSteeringAngle, 0, carData.powerSteering * Time.deltaTime);
        }
        else
        {
            currentSteeringAngle += steeringInput * carData.powerSteering * Time.deltaTime;
        }
        currentSteeringAngle = Mathf.Clamp(currentSteeringAngle, carData.minSteeringAngle, carData.maxSteeringAngle);
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
                    carData.rideHeight + carData.wheelRadius, carData.groundMask))
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
        rb.AddForceAtPosition(carData.gravity*rb.mass*Vector3.down, centerOfMass.position);
    }
    void TireSpringForce(Transform applicableTire, RaycastHit hit)
    {
        Vector3 springDir = hit.normal;
        Vector3 tireVelocity = rb.GetPointVelocity(applicableTire.position);
        float springLength = carData.rideHeight - (hit.distance - carData.wheelRadius);
        float velocityraptor = Vector3.Dot(springDir, tireVelocity);
        float appliedForce = springLength*carData.springStrength - velocityraptor*carData.damper;
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
            GripFactor = carData.frontGripGraph.Evaluate(effectiveVelocityPercentage);
        }
        else
        {
            GripFactor = carData.rearGripGraph.Evaluate(effectiveVelocityPercentage);
        }

        float DesiredVelocityChange = -effectiveVelocity*GripFactor;
        float Acceleration = DesiredVelocityChange/Time.fixedDeltaTime;
        rb.AddForceAtPosition(rb.mass/wheels.Count*Acceleration*tireGripDirection, applicableTire.position);
    }

    void accelerate(Transform applicableTire)
    {
        Vector3 accelerationdirection = applicableTire.forward;
        float currentCarSpeed = Vector3.Dot(transform.forward, rb.linearVelocity);
        float normalizedCarSpeed = Mathf.Clamp01(Mathf.Abs(currentCarSpeed)/carData.gears[currentGear].topSpeed);
        float availeableTorque = carData.gears[currentGear].acceleration.Evaluate(normalizedCarSpeed)*carData.gears[currentGear].scale*forwardInput;
        if (currentCarSpeed < carData.gears[currentGear].topSpeed)
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
        float brakeFactor = carData.braking.Evaluate(effectiveVelocityPercentage);

        float DesiredVelocityChange = effectiveVelocity*brakeFactor*forwardInput;
        float Acceleration = DesiredVelocityChange/Time.fixedDeltaTime;
        rb.AddForceAtPosition(rb.mass/wheels.Count*Acceleration*tireBrakeDirection, applicableTire.position);
    }
}
