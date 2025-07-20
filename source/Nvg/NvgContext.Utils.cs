using NanoUI.Common;
using NanoUI.Nvg.Data;
using NanoUI.Rendering;
using System;
using System.Numerics;

namespace NanoUI.Nvg
{
    partial class NvgContext
    {
        #region Caps

        // nvg__buttCapStart
        static void _buttCapStart(in NvgPoint p, Vector2 delta, float w, float d, float aa, float u0, float u1)
        {
            Vector2 pPos = p.Position - delta * d;
            Vector2 dl = new(delta.Y, -delta.X);

            DrawCache.AddVertex(pPos + dl * w - delta * aa, u0, 0.0f);
            DrawCache.AddVertex(pPos - dl * w - delta * aa, u1, 0.0f);
            DrawCache.AddVertex(pPos + dl * w, u0, 1.0f);
            DrawCache.AddVertex(pPos - dl * w, u1, 1.0f);
        }

        // nvg__buttCapEnd
        static void _buttCapEnd(in NvgPoint p, Vector2 delta, float w, float d, float aa, float u0, float u1)
        {
            Vector2 pPos = p.Position + delta * d;
            Vector2 dl = new(delta.Y, -delta.X);

            DrawCache.AddVertex(pPos + dl * w, u0, 1.0f);
            DrawCache.AddVertex(pPos - dl * w, u1, 1.0f);
            DrawCache.AddVertex(pPos + dl * w + delta * aa, u0, 0.0f);
            DrawCache.AddVertex(pPos - dl * w + delta * aa, u1, 0.0f);
        }

        // nvg__roundCapStart
        static void _roundCapStart(in NvgPoint p, Vector2 delta, float w, uint ncap, float u0, float u1)
        {
            Vector2 pPos = p.Position;
            Vector2 dl = new(delta.Y, -delta.X);

            for (int i = 0; i < ncap; i++)
            {
                float a = i / (float)(ncap - 1) * MathF.PI;
                float ax = MathF.Cos(a) * w;
                float ay = MathF.Sin(a) * w;

                DrawCache.AddVertex(pPos - dl * ax - delta * ay, u0, 1.0f);
                DrawCache.AddVertex(pPos, 0.5f, 1.0f);
            }

            DrawCache.AddVertex(pPos + dl * w, u0, 1.0f);
            DrawCache.AddVertex(pPos - dl * w, u1, 1.0f);
        }

        // nvg__roundCapEnd
        static void _roundCapEnd(in NvgPoint p, Vector2 delta, float w, uint ncap, float u0, float u1)
        {
            Vector2 pPos = p.Position;
            Vector2 dl = new(delta.Y, -delta.X);

            DrawCache.AddVertex(pPos + dl * w, u0, 1.0f);
            DrawCache.AddVertex(pPos - dl * w, u1, 1.0f);

            for (int i = 0; i < ncap; i++)
            {
                float a = i / (float)(ncap - 1) * MathF.PI;
                float ax = MathF.Cos(a) * w;
                float ay = MathF.Sin(a) * w;

                DrawCache.AddVertex(pPos, 0.5f, 1.0f);
                DrawCache.AddVertex(pPos - dl * ax + delta * ay, u0, 1.0f);
            }
        }

        #endregion

        #region Joins

        // nvg__bevelJoin
        static void _bevelJoin(in NvgPoint p0, in NvgPoint p1, float lw, float rw, float lu, float ru)
        {
            Vector2 dl0 = new(p0.Delta.Y, -p0.Delta.X);
            Vector2 dl1 = new(p1.Delta.Y, -p1.Delta.X);

            if ((p1.Flags & NvgPointFlags.Left) != 0)
            {
                _chooseBevel((p1.Flags & NvgPointFlags.InnerBevel) != 0, p0, p1, lw, out Vector2 l0, out Vector2 l1);

                DrawCache.AddVertex(l0, lu, 1.0f);
                DrawCache.AddVertex(p1.Position - dl0 * rw, ru, 1.0f);

                if ((p1.Flags & NvgPointFlags.Bevel) != 0)
                {
                    DrawCache.AddVertex(l0, lu, 1.0f);
                    DrawCache.AddVertex(p1.Position - dl0 * rw, ru, 1.0f);

                    DrawCache.AddVertex(l1, lu, 1.0f);
                    DrawCache.AddVertex(p1.Position - dl1 * rw, ru, 1.0f);
                }
                else
                {
                    Vector2 r0 = p1.Position - p1.DeltaM * rw;

                    DrawCache.AddVertex(p1.Position, 0.5f, 1.0f);
                    DrawCache.AddVertex(p1.Position - dl0 * rw, ru, 1.0f);

                    DrawCache.AddVertex(r0, ru, 1.0f);
                    DrawCache.AddVertex(r0, ru, 1.0f);

                    DrawCache.AddVertex(p1.Position, 0.5f, 1.0f);
                    DrawCache.AddVertex(p1.Position - dl1 * rw, ru, 1.0f);
                }

                DrawCache.AddVertex(l1, lu, 1.0f);
                DrawCache.AddVertex(p1.Position - dl1 * rw, ru, 1.0f);
            }
            else
            {
                _chooseBevel((p1.Flags & NvgPointFlags.InnerBevel) != 0, p0, p1, -rw, out Vector2 r0, out Vector2 r1);

                DrawCache.AddVertex(p1.Position + dl0 * lw, lu, 1.0f);
                DrawCache.AddVertex(r0, ru, 1.0f);

                if ((p1.Flags & NvgPointFlags.Bevel) != 0)
                {
                    DrawCache.AddVertex(p1.Position + dl0 * lw, lu, 1.0f);
                    DrawCache.AddVertex(r0, ru, 1.0f);

                    DrawCache.AddVertex(p1.Position + dl1 * lw, lu, 1.0f);
                    DrawCache.AddVertex(r1, ru, 1.0f);
                }
                else
                {
                    Vector2 l0 = p1.DeltaM * lw;

                    DrawCache.AddVertex(p1.Position + dl0 * lw, lu, 1.0f);
                    DrawCache.AddVertex(p1.Position, 0.5f, 1.0f);

                    DrawCache.AddVertex(l0, lu, 1.0f);
                    DrawCache.AddVertex(l0, lu, 1.0f);

                    DrawCache.AddVertex(p1.Position + dl1 * lw, lu, 1.0f);
                    DrawCache.AddVertex(p1.Position, 0.5f, 1.0f);
                }

                DrawCache.AddVertex(p1.Position + dl1 * lw, lu, 1.0f);
                DrawCache.AddVertex(r1, ru, 1.0f);
            }
        }

        // nvg__roundJoin
        static void _roundJoin(in NvgPoint p0, in NvgPoint p1, float lw, float rw, float lu, float ru, uint ncap)
        {
            Vector2 dl0 = new(p0.Delta.Y, -p0.Delta.X);
            Vector2 dl1 = new(p1.Delta.Y, -p1.Delta.X);

            if ((p1.Flags & NvgPointFlags.Left) != 0)
            {
                _chooseBevel((p1.Flags & NvgPointFlags.InnerBevel) != 0, p0, p1, lw, out Vector2 l0, out Vector2 l1);

                float a0 = MathF.Atan2(-dl0.Y, -dl0.X);
                float a1 = MathF.Atan2(-dl1.Y, -dl1.X);

                if (a1 > a0)
                {
                    a1 -= MathF.PI * 2.0f;
                }

                DrawCache.AddVertex(l0, lu, 1.0f);
                DrawCache.AddVertex(p1.Position - dl0 * rw, ru, 1.0f);

                uint n = (uint)Math.Clamp((int)MathF.Ceiling((a0 - a1) / MathF.PI * ncap), 2, ncap);

                for (int i = 0; i < n; i++)
                {
                    float u = i / (float)(n - 1);
                    float a = a0 + u * (a1 - a0);
                    float rx = p1.Position.X + MathF.Cos(a) * rw;
                    float ry = p1.Position.Y + MathF.Sin(a) * rw;

                    DrawCache.AddVertex(p1.Position, 0.5f, 1.0f);
                    DrawCache.AddVertex(rx, ry, ru, 1.0f);
                }

                DrawCache.AddVertex(l1, lu, 1.0f);
                DrawCache.AddVertex(p1.Position - dl1 * rw, ru, 1.0f);
            }
            else
            {
                _chooseBevel((p1.Flags & NvgPointFlags.InnerBevel) != 0, p0, p1, -rw, out Vector2 r0, out Vector2 r1);

                float a0 = MathF.Atan2(dl0.Y, dl0.X);
                float a1 = MathF.Atan2(dl1.Y, dl1.X);

                if (a1 < a0)
                {
                    a1 += MathF.PI * 2.0f;
                }

                DrawCache.AddVertex(p1.Position + dl0 * rw, lu, 1.0f);
                DrawCache.AddVertex(r0, ru, 1.0f);

                uint n = (uint)Math.Clamp((int)MathF.Ceiling((a1 - a0) / MathF.PI * ncap), 2, ncap);

                for (int i = 0; i < n; i++)
                {
                    float u = i / (float)(n - 1);
                    float a = a0 + u * (a1 - a0);
                    float lx = p1.Position.X + MathF.Cos(a) * lw;
                    float ly = p1.Position.Y + MathF.Sin(a) * lw;

                    DrawCache.AddVertex(lx, ly, lu, 1.0f);
                    DrawCache.AddVertex(p1.Position, 0.5f, 1.0f);
                }

                DrawCache.AddVertex(p1.Position + dl1 * rw, lu, 1.0f);
                DrawCache.AddVertex(r1, ru, 1.0f);
            }
        }

        // nvg__chooseBevel
        static void _chooseBevel(bool bevel, in NvgPoint p0, in NvgPoint p1, float w, out Vector2 pos0, out Vector2 pos1)
        {
            if (bevel)
            {
                pos0 = new(p1.Position.X + p0.Delta.Y * w, p1.Position.Y - p0.Delta.X * w);
                pos1 = new(p1.Position.X + p1.Delta.Y * w, p1.Position.Y - p1.Delta.X * w);
            }
            else
            {
                pos0 = new(p1.Position.X + p1.DeltaM.X * w, p1.Position.Y + p1.DeltaM.Y * w);
                pos1 = new(p1.Position.X + p1.DeltaM.X * w, p1.Position.Y + p1.DeltaM.Y * w);
            }
        }

        #endregion

        #region Common

        // nvg__polyArea
        static float _polyArea(Span<NvgPoint> points)
        {
            float area = 0;

            for (int i = 2; i < points.Length; i++)
            {
                area += _triarea2(points[0].Position, points[i - 1].Position, points[i].Position);
            }

            return area * 0.5f;
        }

        // nvg__polyReverse
        static void _polyReverse(Span<NvgPoint> points)
        {
            int i = 0, j = points.Length - 1;

            while (i < j)
            {
                // swap
                (points[i], points[j]) = (points[j], points[i]);

                i++;
                j--;
            }
        }

        // nvg__isectRects
        static Rect _isectRects(in Rect a, in Rect b)
        {
            Vector2 min = Vector2.Max(a.Position, b.Position);
            Vector2 max = Vector2.Min(a.Max, b.Max);
            return new Rect(min, Vector2.Max(new Vector2(0.0f), max - min));
        }

        // nvg__ptEquals
        static bool _ptEquals(Vector2 p1, Vector2 p2, float tol)
        {
            Vector2 d = p2 - p1;
            return d.X * d.X + d.Y * d.Y < tol * tol;
        }

        // nvg__triarea2
        static float _triarea2(Vector2 a, Vector2 b, Vector2 c)
        {
            Vector2 ab = b - a;
            Vector2 ac = c - a;
            return ac.X * ab.Y - ab.X * ac.Y;
        }

        // nvg__distPtSeg
        static float _distPtSeg(Vector2 pos, Vector2 p, Vector2 q)
        {
            Vector2 pq = q - p;
            Vector2 d = pos - p;
            float delta = pq.X * pq.X + pq.Y * pq.Y;
            float t = pq.X * d.X + pq.Y * d.Y;

            if (delta > 0)
            {
                t /= delta;
            }

            t = Math.Clamp(t, 0.0f, 1.0f);

            d = p + t * pq - pos;

            return d.X * d.X + d.Y * d.Y;
        }

        // nvg__curveDivs
        static uint _curveDivs(float r, float arc, float tol)
        {
            float da = MathF.Acos(r / (r + tol)) * 2.0f;

            return Math.Max(2, (uint)MathF.Ceiling(arc / da));
        }

        #endregion
    }
}