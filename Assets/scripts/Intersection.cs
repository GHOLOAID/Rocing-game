using UnityEngine.Splines;
using System.Collections.Generic;

public class Intersection
{
    public List<IntersectionPoint> points;

    public void AddPoint(int splineIndex, int knotIndex, Spline spline, BezierKnot knot)
    {
        if (points == null)
            points = new List<IntersectionPoint>();
        
        points.Add(new IntersectionPoint(splineIndex, knotIndex, spline, knot));
    }
}

public struct IntersectionPoint
{
    public int splineIndex;
    public int knotIndex;
    public Spline spline;
    public BezierKnot knot;

    public IntersectionPoint(int splineIndex, int knotIndex, Spline spline, BezierKnot knot)
    {
        this.splineIndex = splineIndex;
        this.knotIndex = knotIndex;
        this.spline = spline;
        this.knot = knot;
    }
}
