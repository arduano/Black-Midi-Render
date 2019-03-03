using Poly2Tri.Triangulation;
using Poly2Tri.Triangulation.Polygon;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BMEngine
{
    public static class GLTextEngine
    {
        static bool CheckIfInside(
IList<TriangulationPoint> polygonToTest,
IList<TriangulationPoint> containingPolygon)
        {
            var t = 0;
            for (var i = 0; i < polygonToTest.Count; ++i)
            {
                if (PointInPolygon(polygonToTest[i], containingPolygon)) t++;
            }

            return ((float)t) >= (polygonToTest.Count * .6f) ? true : false;
        }

        static bool PointInPolygon(TriangulationPoint p, IList<TriangulationPoint> poly)
        {
            PolygonPoint p1, p2;
            var inside = false;
            var oldPoint = new PolygonPoint(poly[poly.Count - 1].X, poly[poly.Count - 1].Y);

            for (var i = 0; i < poly.Count; i++)
            {
                var newPoint = new PolygonPoint(poly[i].X, poly[i].Y);
                if (newPoint.X > oldPoint.X) { p1 = oldPoint; p2 = newPoint; }
                else { p1 = newPoint; p2 = oldPoint; }
                if ((newPoint.X < p.X) == (p.X <= oldPoint.X) && ((long)p.Y - (long)p1.Y) * (long)(p2.X - p1.X)
                     < ((long)p2.Y - (long)p1.Y) * (long)(p.X - p1.X))
                {
                    inside = !inside;
                }
                oldPoint = newPoint;
            }
            return inside;
        }

        class PolygonHierachy
        {
            public Polygon Current;
            public List<PolygonHierachy> Childs;
            public PolygonHierachy Next;

            public PolygonHierachy(Polygon current)
            {
                Current = current;
                Childs = new List<PolygonHierachy>();
                Next = null;
            }
        }

        static void ProcessLevel(Polygon poly, ref PolygonHierachy localRoot)
        {
            if (localRoot == null)
            {
                localRoot = new PolygonHierachy(poly);
                return;
            }

            if (CheckIfInside(localRoot.Current.Points, poly.Points))
            {
                var nroot = new PolygonHierachy(poly);
                var tmp = localRoot;
                while (tmp != null)
                {
                    var cur = tmp;
                    tmp = tmp.Next;
                    cur.Next = null;
                    nroot.Childs.Add(cur);
                }

                localRoot = nroot;
                return;
            }

            if (!CheckIfInside(poly.Points, localRoot.Current.Points))
            {
                ProcessLevel(poly, ref localRoot.Next);
                return;
            }

            for (var i = 0; i < localRoot.Childs.Count; ++i)
            {
                if (!CheckIfInside(poly.Points, localRoot.Childs[i].Current.Points)) continue;

                var childRoot = localRoot.Childs[i];
                ProcessLevel(poly, ref childRoot);
                localRoot.Childs[i] = childRoot;
                return;
            }

            var newChildList = new List<PolygonHierachy>();
            var newPoly = new PolygonHierachy(poly);
            newChildList.Add(newPoly);
            for (var i = 0; i < localRoot.Childs.Count; ++i)
            {
                if (CheckIfInside(localRoot.Childs[i].Current.Points, poly.Points))
                {
                    newPoly.Childs.Add(localRoot.Childs[i]);
                }
                else
                {
                    newChildList.Add(localRoot.Childs[i]);
                }
            }

            localRoot.Childs = newChildList;
        }


        public static List<Polygon> GetTextPolygons()
        {
            var path = new GraphicsPath();

            path.AddString("TEXT!!!", new FontFamily(GenericFontFamilies.SansSerif), (int)System.Drawing.FontStyle.Regular, 100,
                new PointF(0f, 0f), StringFormat.GenericDefault);

            path.Flatten();

            if (path.PointCount == 0) return new List<Polygon>();
            var pts = path.PathPoints;
            var ptsType = path.PathTypes;
            path.Dispose();


            var polygons = new List<Polygon>();
            List<PolygonPoint> points = null;
            Pen cPen = Pens.Yellow;
            var start = -1;

            for (var i = 0; i < pts.Length; i++)
            {
                var pointType = ptsType[i] & 0x07;
                if (pointType == 0)
                {
                    points = new List<PolygonPoint> { new PolygonPoint(pts[i].X, pts[i].Y) };
                    start = i;
                    continue;
                }
                if (pointType != 1) throw new Exception("Unsupported point type");

                if ((ptsType[i] & 0x80) != 0)
                {
                    //- Last point in the polygon
                    if (pts[i] != pts[start])
                    {
                        points.Add(new PolygonPoint(pts[i].X, pts[i].Y));
                    }
                    polygons.Add(new Polygon(points));

                    points = null;
                }
                else
                {
                    points.Add(new PolygonPoint(pts[i].X, pts[i].Y));
                }
            }
            return polygons;
        }
    }
}
