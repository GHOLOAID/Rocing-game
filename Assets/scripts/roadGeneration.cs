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
    List<Vector3> left;
    List<Vector3> right;

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
}
