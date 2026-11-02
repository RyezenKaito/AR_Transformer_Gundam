using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Procedural meshes for the four AR shapes: Torus, Cone, Triangular Prism, Dodecahedron.
/// Every mesh has its pivot at the center of its bottom, so it sits on top of an AR plane.
/// </summary>
public static class ShapeMeshFactory
{
    class Builder
    {
        public readonly List<Vector3> v = new List<Vector3>();
        public readonly List<Vector3> n = new List<Vector3>();
        public readonly List<int> t = new List<int>();

        // Triangle with per-vertex normals. Winding is fixed so the face looks along the average normal.
        public void Tri(Vector3 a, Vector3 b, Vector3 c, Vector3 na, Vector3 nb, Vector3 nc)
        {
            Vector3 face = Vector3.Cross(b - a, c - a);
            if (Vector3.Dot(face, na + nb + nc) < 0f)
            {
                Vector3 tp = b; b = c; c = tp;
                Vector3 tn = nb; nb = nc; nc = tn;
            }
            int i = v.Count;
            v.Add(a); v.Add(b); v.Add(c);
            n.Add(na); n.Add(nb); n.Add(nc);
            t.Add(i); t.Add(i + 1); t.Add(i + 2);
        }

        // Flat shaded triangle of a convex solid. Outward is the direction from the solid center to the face.
        public void FlatTri(Vector3 a, Vector3 b, Vector3 c, Vector3 solidCenter)
        {
            Vector3 face = Vector3.Cross(b - a, c - a).normalized;
            Vector3 outward = (a + b + c) / 3f - solidCenter;
            if (Vector3.Dot(face, outward) < 0f) face = -face;
            Tri(a, b, c, face, face, face);
        }

        public void FlatQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 solidCenter)
        {
            FlatTri(a, b, c, solidCenter);
            FlatTri(a, c, d, solidCenter);
        }

        public Mesh ToMesh(string name)
        {
            float minY = float.MaxValue;
            for (int i = 0; i < v.Count; i++) minY = Mathf.Min(minY, v[i].y);
            for (int i = 0; i < v.Count; i++) v[i] = new Vector3(v[i].x, v[i].y - minY, v[i].z);

            var m = new Mesh();
            m.name = name;
            m.SetVertices(v);
            m.SetNormals(n);
            m.SetTriangles(t, 0);
            m.RecalculateBounds();
            return m;
        }
    }

    public static Mesh CreateTorus(float majorRadius = 0.05f, float minorRadius = 0.02f, int majorSegments = 48, int minorSegments = 24)
    {
        var mb = new Builder();
        int stride = minorSegments + 1;
        var pos = new Vector3[(majorSegments + 1) * stride];
        var nor = new Vector3[pos.Length];

        for (int i = 0; i <= majorSegments; i++)
        {
            float u = 2f * Mathf.PI * i / majorSegments;
            float cu = Mathf.Cos(u), su = Mathf.Sin(u);
            for (int j = 0; j <= minorSegments; j++)
            {
                float w = 2f * Mathf.PI * j / minorSegments;
                float cw = Mathf.Cos(w), sw = Mathf.Sin(w);
                float ring = majorRadius + minorRadius * cw;
                pos[i * stride + j] = new Vector3(ring * cu, minorRadius * sw, ring * su);
                nor[i * stride + j] = new Vector3(cw * cu, sw, cw * su);
            }
        }

        for (int i = 0; i < majorSegments; i++)
        {
            for (int j = 0; j < minorSegments; j++)
            {
                int a = i * stride + j;
                int b = a + stride;
                int c = b + 1;
                int d = a + 1;
                mb.Tri(pos[a], pos[b], pos[d], nor[a], nor[b], nor[d]);
                mb.Tri(pos[b], pos[c], pos[d], nor[b], nor[c], nor[d]);
            }
        }
        return mb.ToMesh("Torus");
    }

    public static Mesh CreateCone(float radius = 0.05f, float height = 0.12f, int segments = 48)
    {
        var mb = new Builder();
        Vector3 apex = new Vector3(0f, height, 0f);
        Vector3 down = Vector3.down;

        for (int i = 0; i < segments; i++)
        {
            float a0 = 2f * Mathf.PI * i / segments;
            float a1 = 2f * Mathf.PI * (i + 1) / segments;
            Vector3 p0 = new Vector3(radius * Mathf.Cos(a0), 0f, radius * Mathf.Sin(a0));
            Vector3 p1 = new Vector3(radius * Mathf.Cos(a1), 0f, radius * Mathf.Sin(a1));

            mb.Tri(apex, p0, p1, SideNormal((a0 + a1) * 0.5f, radius, height), SideNormal(a0, radius, height), SideNormal(a1, radius, height));
            mb.Tri(Vector3.zero, p0, p1, down, down, down);
        }
        return mb.ToMesh("Cone");
    }

    static Vector3 SideNormal(float angle, float radius, float height)
    {
        return new Vector3(Mathf.Cos(angle) * height, radius, Mathf.Sin(angle) * height).normalized;
    }

    public static Mesh CreatePrism(float side = 0.1f, float length = 0.1f)
    {
        var mb = new Builder();
        float hw = side * 0.5f;
        float h = side * Mathf.Sqrt(3f) * 0.5f;
        float l = length * 0.5f;

        Vector3 a = new Vector3(-hw, 0f, l), b = new Vector3(hw, 0f, l), c = new Vector3(0f, h, l);
        Vector3 a2 = new Vector3(-hw, 0f, -l), b2 = new Vector3(hw, 0f, -l), c2 = new Vector3(0f, h, -l);
        Vector3 center = new Vector3(0f, h / 3f, 0f);

        mb.FlatTri(a, b, c, center);
        mb.FlatTri(a2, b2, c2, center);
        mb.FlatQuad(a, b, b2, a2, center);
        mb.FlatQuad(b, c, c2, b2, center);
        mb.FlatQuad(c, a, a2, c2, center);
        return mb.ToMesh("TriangularPrism");
    }

    public static Mesh CreateDodecahedron(float radius = 0.06f)
    {
        var mb = new Builder();
        float phi = (1f + Mathf.Sqrt(5f)) * 0.5f;
        float inv = 1f / phi;
        float scale = radius / Mathf.Sqrt(3f);

        // Rotate so that one pentagon lies flat on the ground.
        Quaternion q = Quaternion.FromToRotation(new Vector3(0f, phi, 1f).normalized, Vector3.up);

        var verts = new List<Vector3>();
        float[] signs = { -1f, 1f };
        foreach (float sx in signs)
            foreach (float sy in signs)
                foreach (float sz in signs)
                    verts.Add(new Vector3(sx, sy, sz));
        foreach (float s1 in signs)
        {
            foreach (float s2 in signs)
            {
                verts.Add(new Vector3(0f, s1 * inv, s2 * phi));
                verts.Add(new Vector3(s1 * inv, s2 * phi, 0f));
                verts.Add(new Vector3(s2 * phi, 0f, s1 * inv));
            }
        }
        for (int i = 0; i < verts.Count; i++) verts[i] = q * (verts[i] * scale);

        var normals = new List<Vector3>();
        foreach (float s1 in signs)
        {
            foreach (float s2 in signs)
            {
                normals.Add(new Vector3(0f, s1 * phi, s2));
                normals.Add(new Vector3(s2, 0f, s1 * phi));
                normals.Add(new Vector3(s1 * phi, s2, 0f));
            }
        }

        foreach (Vector3 nRaw in normals)
        {
            Vector3 nrm = (q * nRaw).normalized;
            float best = float.MinValue;
            foreach (Vector3 p in verts) best = Mathf.Max(best, Vector3.Dot(p, nrm));

            var face = new List<Vector3>();
            foreach (Vector3 p in verts)
                if (Vector3.Dot(p, nrm) > best - radius * 0.01f) face.Add(p);

            Vector3 c = Vector3.zero;
            foreach (Vector3 p in face) c += p;
            c /= face.Count;

            Vector3 u = (face[0] - c).normalized;
            Vector3 w = Vector3.Cross(nrm, u);
            face.Sort((p1, p2) =>
                Mathf.Atan2(Vector3.Dot(p1 - c, w), Vector3.Dot(p1 - c, u))
                    .CompareTo(Mathf.Atan2(Vector3.Dot(p2 - c, w), Vector3.Dot(p2 - c, u))));

            for (int i = 1; i < face.Count - 1; i++)
                mb.FlatTri(face[0], face[i], face[i + 1], Vector3.zero);
        }
        return mb.ToMesh("Dodecahedron");
    }
}
