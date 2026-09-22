using UnityEngine;

[CreateAssetMenu(fileName = "carSettijgs", menuName = "funny knobs moment")]
public class carSettijgs : ScriptableObject
{
    public float rideHeight;
    public float gravity;
    public float springStrength;
    public float damper;
    public float maxGroundDistance;
    public LayerMask groundMask;
}
