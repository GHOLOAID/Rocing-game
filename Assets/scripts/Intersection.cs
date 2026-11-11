using UnityEngine.Splines;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class Intersection
{
    public List<IntersectionPoint> points;
    public List<float> curves;

    public void AddPoint(int splineIndex, int knotIndex, Spline spline, BezierKnot knot)
    {
        if (points == null)
        {
            points = new List<IntersectionPoint>();
            curves = new List<float>();
        }
        
        points.Add(new IntersectionPoint(splineIndex, knotIndex, spline, knot));
        curves.Add(0.2f);
    }
}

[Serializable]
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

struct IntersectionEdge
{
    public Vector3 left;
    public Vector3 right;
    public Vector3 center => (left + right)/2;
    
    public IntersectionEdge(Vector3 left, Vector3 right)
    {
        this.left = left;
        this.right = right;
    }

}
