using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteAlways]
public class roadGeneration : MonoBehaviour
{
    [SerializeField] int res = 20; //points considered around spline while making the road
    [SerializeField] float width = 2;
    [SerializeField] SplineSampler sampler;
    [SerializeField] MeshFilter filter;
    [SerializeField] List<Intersection> intersections = new List<Intersection>();
    List<Vector3> left;
    List<Vector3> right;
    [SerializeField] [Range(0.01f, 1f)] float step = 0.05f;

    private void GetRoadSections()
    {
        left = new List<Vector3>();
        right = new List<Vector3>();
        float step = 1f / (float)res;
        Vector3 l;
        Vector3 r;
        for (int i = 0; i < sampler.splineCount; i++)
        {
            for (int j = 0; j < res; j++)
            {
                float t = step * j;
                sampler.Sample(i, t, width, out Vector3 _, out l, out r);
                left.Add(l);
                right.Add(r);
            }
            sampler.Sample(i, 1f, width, out Vector3 _, out l, out r);
            left.Add(l);
            right.Add(r);
        }
    }

    private void BuildTheMesh()
    {
        Mesh m = new Mesh();
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        int offset = 0;
        int len = left.Count;
        for (int i = 0; i < sampler.splineCount; i++)
        {
            int splineOffset = res*i + i;
            for (int j = 1; j <= res; j++)
            {
                int vertOff = splineOffset + j;
                Vector3 l1 = left[vertOff-1]; //one square road segment
                Vector3 r1 = right[vertOff-1];
                Vector3 l2 = left[vertOff];
                Vector3 r2 = right[vertOff];
                offset = 4*res*i /*previous splines*/+4*(j-1);//current splines
                int tri1 = offset + 0;
                int tri2 = offset + 2;
                int tri3 = offset + 3;
                int tri4 = offset + 3;
                int tri5 = offset + 1;
                int tri6 = offset + 0;

                verts.AddRange(new List<Vector3>{r1, l1, r2, l2});
                tris.AddRange(new List<int>{tri1, tri2, tri3, tri4, tri5, tri6});
            }
        }

        offset = verts.Count;
        for (int i = 0; i < intersections.Count; i++)
        {
            int count = 0;
            List<IntersectionEdge> intersectionEdges = new List<IntersectionEdge>();
            Vector3 center = Vector3.zero;
            for(int j = 0; j < intersections[i].points.Count; j++)
            {
                float t  = intersections[i].points[j].knotIndex == 0 ? 0f : 1f;
                sampler.Sample(intersections[i].points[j].splineIndex, t, width, out Vector3 _, out Vector3 pLeft, out Vector3 pRight);
                if (intersections[i].points[j].knotIndex == 0)
                {
                    intersectionEdges.Add(new IntersectionEdge(pLeft, pRight));
                }
                else
                {
                    intersectionEdges.Add(new IntersectionEdge(pRight, pLeft));
                }
                center += pLeft;
                center += pRight;
                count ++;
            }
            center = center/(count*2);

            intersectionEdges.Sort(
                (p1, p2) =>
                {
                    Vector3 p1Dir = p1.center - center;
                    float p1Angle = Vector3.SignedAngle(center.normalized, p1Dir.normalized, Vector3.up);
                    Vector3 p2Dir = p2.center - center;
                    float p2Angle = Vector3.SignedAngle(center.normalized, p2Dir.normalized, Vector3.up);
                    if (p1Angle > p2Angle)
                    {
                        return 1;
                    }
                    if (p1Angle < p2Angle)
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            );

            List<Vector3> curvePoints = new List<Vector3>();
            Vector3 mid;
            Vector3 a;
            Vector3 b;
            Vector3 c;
            BezierCurve curve;
            for (int j = 1; j <= intersectionEdges.Count; j++)
            {
                a = intersectionEdges[j-1].left;
                curvePoints.Add(a);
                b = j < intersectionEdges.Count ? intersectionEdges[j].right : intersectionEdges[0].right;
                mid = Vector3.Lerp(a, b, 0.5f);
                c = Vector3.Lerp(mid, center, 0.3f);
                curve = new BezierCurve(a, c, b);

                for (float t = step; t < 1; t += step)
                {
                    Vector3 position = CurveUtility.EvaluatePosition(curve, t);
                    curvePoints.Add(position);
                }
                curvePoints.Add(b);
            }

            for (int j = 1; j <= curvePoints.Count; j++)
            {
                verts.Add(center);
                verts.Add(curvePoints[j-1]);
                if (j == curvePoints.Count)
                {
                    verts.Add(curvePoints[0]);
                }
                else
                {
                    verts.Add(curvePoints[j]);
                }
                tris.Add(offset + ((j-1)*3)+0);
                tris.Add(offset + ((j-1)*3)+1);
                tris.Add(offset + ((j-1)*3)+2);
                
            }
        }

        m.SetVertices(verts);
        m.SetTriangles(tris, 0);
        filter.mesh = m;
    }
    
    private void Rebuild()
    {
        if (sampler == null || filter == null || sampler.splineCount == 0)
        {
            return;
        }
        GetRoadSections();
        BuildTheMesh();
    }
    
    private void OnValidate()
    {
        Rebuild();
    }
    
    private void OnEnable()
    {
        Spline.Changed += onSplineChanged;
    }
    
    private void onSplineChanged(Spline s, int i, SplineModification mod)
    {
        Rebuild();
    }
    
    private void OnDisable()
    {
        Spline.Changed -= onSplineChanged;
    }
    
    private void Awake()
    {
        Rebuild();
    }

    public void BuildIntersection(Intersection intersection)
    {
        intersections.Add(intersection);
        Rebuild();
    }
}
