using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "carSettings", menuName = "funny knobs moment")]
public class carSettings : ScriptableObject
{
    [Header("suspension")]
    // suspension
    public float rideHeight;
    public float springStrength;
    public float damper;
    [Space]
    [Header("miscellaneous")]
    // misc
    public float gravity;
    public float wheelRadius;
    public LayerMask groundMask;
    [Space]
    [Header("steering")]
    // steering
    public float minSteeringAngle;
    public float maxSteeringAngle;
    public float powerSteering;
    public AnimationCurve frontGripGraph;
    public AnimationCurve rearGripGraph;
    [Space]
    [Header("acceleration")]
    public List <Gear> gears;
    public AnimationCurve braking;
}

[System.Serializable]
public class Gear
{
    public float scale;
    public AnimationCurve acceleration;
    public float topSpeed;
}