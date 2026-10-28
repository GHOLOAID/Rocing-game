using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class SplineSampler : MonoBehaviour
{
    [SerializeField] SplineContainer splineContainer;
    private float3 location;
    private float3 facing;
    private float3 up;
    public int splineCount => splineContainer != null ? splineContainer.Splines.Count : 0;


    /// <summary>
    /// figures out the details of the spline at the given point
    /// </summary>
    /// <param name="index">index of spline container</param>
    /// <param name="t">% of way thru slpine (0-1)</param>
    /// <param name="w">width</param>
    /// <param name="pos">location of point on spline</param>
    /// <param name="pLeft">left wall of the road at this point</param>
    /// <param name="pRight">right wall of the road at this point</param>
    public void Sample(int index, float t, float w, out Vector3 pos, out Vector3 pLeft, out Vector3 pRight)
    {
        splineContainer.Evaluate(index, t, out location, out facing, out up);
        Vector3 worldPos = splineContainer.transform.TransformPoint(location);
        Vector3 worldFacing = splineContainer.transform.TransformDirection(facing);
        Vector3 worldUp = splineContainer.transform.TransformDirection(up);
        Vector3 worldRight = Vector3.Cross(worldFacing, worldUp).normalized;
        Vector3 worldPRight = worldPos+worldRight*(w/2f);
        Vector3 worldPLeft = worldPos-worldRight*(w/2f);
        pos = location;
        pRight = splineContainer.transform.InverseTransformPoint(worldPRight);
        pLeft = splineContainer.transform.InverseTransformPoint(worldPLeft);
    }
}
