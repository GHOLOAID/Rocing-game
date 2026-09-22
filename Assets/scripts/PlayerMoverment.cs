using UnityEngine;
using System.Collections.Generic;

public class PlayerMoverment : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] List <Transform> wheels;
    [SerializeField] carSettijgs questionable;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        foreach (Transform i in wheels)
        {
            RaycastHit hit;
            if (Physics.Raycast(i.position, -i.up, out hit, questionable.maxGroundDistance, questionable.groundMask))
            {
                TireSpringForce(i, hit.distance);
            }
            else
            {
                TireGravity(i);
            }
        }
    }

    void TireGravity(Transform applicableTire)
    {
        rb.AddForceAtPosition(questionable.gravity*rb.mass*Vector3.down, applicableTire.position);
    }

    void TireSpringForce(Transform applicableTire, float distance)
    {
        Vector3 springDir = applicableTire.up;
        Vector3 tireVelocity = rb.GetPointVelocity(applicableTire.position);
        float springLength = questionable.rideHeight - distance;
        float velocityraptor = Vector3.Dot(springDir, tireVelocity);
        float appliedForce = springLength*questionable.springStrength - velocityraptor*questionable.damper;
        rb.AddForceAtPosition(appliedForce*springDir, applicableTire.position);
    }
}
