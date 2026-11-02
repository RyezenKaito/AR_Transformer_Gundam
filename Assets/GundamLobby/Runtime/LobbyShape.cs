using UnityEngine;
using UnityEngine.UI;

namespace GundamLobby
{
    [RequireComponent(typeof(CanvasRenderer))]
    [AddComponentMenu("Gundam Lobby/HUD Shape")]
    public class LobbyShape : MaskableGraphic
    {
        public enum Geometry { Panel, Tab, Hexagon, Diamond, Radar, Grid, Crosshair, Box, Slant, Circle, Triangle, Ring, Corners, Bust }
        public Geometry geometry;
        public Color border = new Color(.12f, .6f, .75f, .6f);
        public float cut = 14f;
        public float thickness = 1f;
        [Header("Gradient fill")]
        public bool gradient;
        public Color gradientEnd = Color.clear;
        public bool gradientVertical;
        [Header("Radar chart (values 0..1, one per axis, clockwise from top)")]
        public float[] radarValues = { .8f, .4f, .2f, 1f, .4f, .4f, .8f };
        public int radarRings = 5;
        public Color radarOutline = new Color(.35f, .85f, .95f, .95f);

        Rect cached;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = rectTransform.rect;
            cached = r;
            if (geometry == Geometry.Grid)
            {
                for (float x = r.xMin; x <= r.xMax; x += 28) Line(vh, new Vector2(x, r.yMin), new Vector2(x, r.yMax), color, .6f);
                for (float y = r.yMin; y <= r.yMax; y += 28) Line(vh, new Vector2(r.xMin, y), new Vector2(r.xMax, y), color, .6f);
                return;
            }
            if (geometry == Geometry.Radar)
            {
                int n = Mathf.Max(3, radarValues != null ? radarValues.Length : 7);
                Vector2 center = r.center;
                float radius = Mathf.Min(r.width, r.height) * .5f - thickness;
                int rings = Mathf.Max(1, radarRings);
                for (int ring = 1; ring <= rings; ring++)
                    Outline(vh, Radial(center, radius * ring / rings, n), ring == rings ? border : border * new Color(1, 1, 1, .55f), ring == rings ? 1.2f : 1f);
                Vector2[] outer = Radial(center, radius, n);
                for (int i = 0; i < n; i++) Line(vh, center, outer[i], border * new Color(1, 1, 1, .5f), .8f);
                Vector2[] data = new Vector2[n];
                for (int i = 0; i < n; i++) data[i] = center + (outer[i] - center) * Mathf.Clamp01(radarValues[i]);
                FillPlain(vh, data, color);
                Outline(vh, data, radarOutline, 1.6f);
                return;
            }
            if (geometry == Geometry.Crosshair)
            {
                float a = Mathf.Min(r.width, r.height) * .25f;
                Line(vh, r.center + Vector2.left * a, r.center + Vector2.right * a, color, thickness);
                Line(vh, r.center + Vector2.down * a, r.center + Vector2.up * a, color, thickness);
                return;
            }
            if (geometry == Geometry.Corners)
            {
                float c = Mathf.Min(cut, Mathf.Min(r.width, r.height) * .5f);
                float t = Mathf.Max(1, thickness);
                Line(vh, new Vector2(r.xMin, r.yMax), new Vector2(r.xMin + c, r.yMax), color, t); Line(vh, new Vector2(r.xMin, r.yMax), new Vector2(r.xMin, r.yMax - c), color, t);
                Line(vh, new Vector2(r.xMax, r.yMax), new Vector2(r.xMax - c, r.yMax), color, t); Line(vh, new Vector2(r.xMax, r.yMax), new Vector2(r.xMax, r.yMax - c), color, t);
                Line(vh, new Vector2(r.xMin, r.yMin), new Vector2(r.xMin + c, r.yMin), color, t); Line(vh, new Vector2(r.xMin, r.yMin), new Vector2(r.xMin, r.yMin + c), color, t);
                Line(vh, new Vector2(r.xMax, r.yMin), new Vector2(r.xMax - c, r.yMin), color, t); Line(vh, new Vector2(r.xMax, r.yMin), new Vector2(r.xMax, r.yMin + c), color, t);
                return;
            }
            if (geometry == Geometry.Bust)
            {
                Vector2[] sh = {
                    new Vector2(r.xMin, r.yMin), new Vector2(r.xMax, r.yMin),
                    new Vector2(r.xMax, r.yMin + r.height * .26f), new Vector2(r.xMin + r.width * .88f, r.yMin + r.height * .40f),
                    new Vector2(r.xMin + r.width * .66f, r.yMin + r.height * .47f), new Vector2(r.xMin + r.width * .34f, r.yMin + r.height * .47f),
                    new Vector2(r.xMin + r.width * .12f, r.yMin + r.height * .40f), new Vector2(r.xMin, r.yMin + r.height * .26f) };
                Fill(vh, sh);
                float hr = Mathf.Min(r.width * .21f, r.height * .2f);
                Fill(vh, Radial(new Vector2(r.center.x, r.yMin + r.height * .70f), hr, 28));
                Fill(vh, new[] {
                    new Vector2(r.center.x - hr * .55f, r.yMin + r.height * .47f), new Vector2(r.center.x + hr * .55f, r.yMin + r.height * .47f),
                    new Vector2(r.center.x + hr * .55f, r.yMin + r.height * .60f), new Vector2(r.center.x - hr * .55f, r.yMin + r.height * .60f) });
                return;
            }
            Vector2[] p;
            float cc = Mathf.Min(cut, Mathf.Min(r.width, r.height) * .4f);
            if (geometry == Geometry.Hexagon) p = Radial(r.center, Mathf.Min(r.width, r.height) * .48f, 6);
            else if (geometry == Geometry.Circle || geometry == Geometry.Ring) p = Radial(r.center, Mathf.Min(r.width, r.height) * .5f - thickness, 40);
            else if (geometry == Geometry.Diamond) p = new[] { new Vector2(r.center.x, r.yMax), new Vector2(r.xMax, r.center.y), new Vector2(r.center.x, r.yMin), new Vector2(r.xMin, r.center.y) };
            else if (geometry == Geometry.Triangle) p = new[] { new Vector2(r.xMin, r.yMax), new Vector2(r.xMax, r.yMax), new Vector2(r.center.x, r.yMin) };
            else if (geometry == Geometry.Tab) p = new[] { new Vector2(r.xMin + cc, r.yMin), new Vector2(r.xMax - cc, r.yMin), new Vector2(r.xMax, r.yMax), new Vector2(r.xMin, r.yMax) };
            else if (geometry == Geometry.Slant) p = new[] { new Vector2(r.xMin, r.yMin), new Vector2(r.xMax - cc, r.yMin), new Vector2(r.xMax, r.yMax), new Vector2(r.xMin + cc, r.yMax) };
            else if (geometry == Geometry.Box) p = new[] { new Vector2(r.xMin, r.yMin), new Vector2(r.xMax, r.yMin), new Vector2(r.xMax, r.yMax), new Vector2(r.xMin, r.yMax) };
            else p = new[] { new Vector2(r.xMin, r.yMin + cc), new Vector2(r.xMin + cc, r.yMin), new Vector2(r.xMax, r.yMin), new Vector2(r.xMax, r.yMax - cc), new Vector2(r.xMax - cc, r.yMax), new Vector2(r.xMin, r.yMax) };
            if (geometry != Geometry.Ring) Fill(vh, p);
            if (thickness > 0) Outline(vh, p, border, thickness);
        }

        Color Tint(Vector2 pt)
        {
            if (!gradient) return color;
            float t = gradientVertical ? Mathf.InverseLerp(cached.yMax, cached.yMin, pt.y) : Mathf.InverseLerp(cached.xMin, cached.xMax, pt.x);
            return Color.Lerp(color, gradientEnd, t);
        }
        void Fill(VertexHelper vh, Vector2[] p)
        {
            int offset = vh.currentVertCount;
            for (int i = 0; i < p.Length; i++) vh.AddVert(p[i], Tint(p[i]), Vector2.zero);
            for (int i = 1; i < p.Length - 1; i++) vh.AddTriangle(offset, offset + i, offset + i + 1);
        }
        static Vector2[] Radial(Vector2 center, float radius, int count)
        {
            var points = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                float a = (90 - i * 360f / count) * Mathf.Deg2Rad;
                points[i] = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
            }
            return points;
        }
        static void FillPlain(VertexHelper vh, Vector2[] p, Color c)
        {
            int offset = vh.currentVertCount;
            for (int i = 0; i < p.Length; i++) vh.AddVert(p[i], c, Vector2.zero);
            for (int i = 1; i < p.Length - 1; i++) vh.AddTriangle(offset, offset + i, offset + i + 1);
        }
        static void Outline(VertexHelper vh, Vector2[] p, Color c, float width)
        {
            for (int i = 0; i < p.Length; i++) Line(vh, p[i], p[(i + 1) % p.Length], c, width);
        }
        static void Line(VertexHelper vh, Vector2 a, Vector2 b, Color c, float width)
        {
            Vector2 n = new Vector2(-(b - a).y, (b - a).x).normalized * width * .5f;
            int i = vh.currentVertCount;
            vh.AddVert(a - n, c, Vector2.zero); vh.AddVert(a + n, c, Vector2.zero);
            vh.AddVert(b + n, c, Vector2.zero); vh.AddVert(b - n, c, Vector2.zero);
            vh.AddTriangle(i, i + 1, i + 2); vh.AddTriangle(i, i + 2, i + 3);
        }
    }
}
