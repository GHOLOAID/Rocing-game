using UnityEngine;

[CreateAssetMenu(fileName = "carSettijgs", menuName = "funny knobs moment")]
public class carSettijgs : ScriptableObject
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

}
