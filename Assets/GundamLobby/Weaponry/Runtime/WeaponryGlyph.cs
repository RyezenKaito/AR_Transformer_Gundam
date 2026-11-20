using UnityEngine;
using UnityEngine.UI;

namespace GundamLobby
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class WeaponryGlyph : MaskableGraphic
    {
        public enum Kind { Rifle, Salvo, Burst, Reactor, Shield }
        public Kind kind;
        public float lineWidth = 1.6f;
        Vector2 P(float x, float y)
        {
            Rect r = rectTransform.rect;
            return new Vector2(r.xMin + x / 132 * r.width, r.yMax - y / 96 * r.height);
        }
        void Path(VertexHelper vh, params float[] xy)
        {
            for (int i = 2; i < xy.Length; i += 2)
                Line(vh, P(xy[i - 2], xy[i - 1]), P(xy[i], xy[i + 1]));
        }
        void Line(VertexHelper vh, Vector2 a, Vector2 b)
        {
            Vector2 n = new Vector2(a.y - b.y, b.x - a.x).normalized * lineWidth * .5f;
            int i = vh.currentVertCount;
            vh.AddVert(a - n, color, Vector2.zero); vh.AddVert(a + n, color, Vector2.zero);
            vh.AddVert(b + n, color, Vector2.zero); vh.AddVert(b - n, color, Vector2.zero);
            vh.AddTriangle(i, i + 1, i + 2); vh.AddTriangle(i, i + 2, i + 3);
        }
        void Circle(VertexHelper vh, float x, float y, float radius)
        {
            for (int i = 0; i < 32; i++)
            {
                float a = i * Mathf.PI / 16, b = (i + 1) * Mathf.PI / 16;
                Line(vh, P(x + Mathf.Cos(a) * radius, y + Mathf.Sin(a) * radius), P(x + Mathf.Cos(b) * radius, y + Mathf.Sin(b) * radius));
            }
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            switch (kind)
            {
                case Kind.Rifle:
                    Path(vh, 8,49,21,36,53,36,64,27,94,27,94,35,119,35,119,44,90,44,81,55,47,55,42,70,31,70,34,55,16,55,8,49);
                    Path(vh,45,36,45,28,67,28,67,36); Path(vh,57,55,61,67,72,67,67,55); Path(vh,19,43,77,43); Path(vh,105,35,105,44); break;
                case Kind.Salvo:
                    Path(vh,15,39,35,20,72,26,85,46,65,65,28,59,15,39); Path(vh,28,27,38,45,76,51); Path(vh,38,45,28,59); Path(vh,35,20,48,38,85,46);
                    Path(vh,55,23,72,5); Path(vh,68,30,89,11); Path(vh,81,39,108,20); Path(vh,94,49,124,34); break;
                case Kind.Burst:
                    Path(vh,66,4,73,28,95,18,85,41,112,47,86,57,97,80,74,68,66,93,57,68,34,80,44,57,18,47,45,40,35,18,58,29,66,4);
                    Circle(vh,66,48,17); Circle(vh,66,48,7); break;
                case Kind.Reactor:
                    Path(vh,66,5,103,26,103,69,66,91,29,69,29,26,66,5); Path(vh,66,16,92,32,92,63,66,79,40,63,40,32,66,16);
                    Circle(vh,66,48,16); Circle(vh,66,48,7); Path(vh,66,25,66,35); Path(vh,66,61,66,72); Path(vh,43,48,53,48); Path(vh,80,48,90,48); break;
                case Kind.Shield:
                    Path(vh,66,7,108,22,100,63,66,91,32,63,24,22,66,7); Path(vh,66,20,93,30,87,56,66,74,45,56,39,30,66,20);
                    Path(vh,50,51,61,61,82,38); break;
            }
        }
    }
}
