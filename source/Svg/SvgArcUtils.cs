using System;

namespace NanoUI.Svg
{
    // Convert an arc to a sequence of cubic bézier curves
    // Credit: https://github.com/fontello/svgpath/blob/master/lib/a2c.js
    internal static class SvgArcUtils
    {
        const float Tau = MathF.PI * 2;

        public static ReadOnlySpan<float[]> ToCurve(float x1, float y1, float x2, float y2, float fa, float fs, float rx,
            float ry, float phi)
        {
            var result = Array.Empty<float[]>();
            var sinPhi = MathF.Sin(phi * Tau / 360);
            var cosPhi = MathF.Cos(phi * Tau / 360);

            // Make sure radii are valid
            var x1P = cosPhi * (x1 - x2) / 2 + sinPhi * (y1 - y2) / 2;
            var y1P = -sinPhi * (x1 - x2) / 2 + cosPhi * (y1 - y2) / 2;

            if (x1P == 0 && y1P == 0)
            {
                // we're asked to draw line to itself
                return result;
            }

            if (rx == 0 || ry == 0)
            {
                // one of the radii is zero
                return result;
            }

            // Compensate out-of-range radii
            rx = MathF.Abs(rx);
            ry = MathF.Abs(ry);

            var lambda = x1P * x1P / (rx * rx) + y1P * y1P / (ry * ry);
            if (lambda > 1)
            {
                rx *= MathF.Sqrt(lambda);
                ry *= MathF.Sqrt(lambda);
            }

            // Get center parameters (cx, cy, theta1, delta_theta)
            var cc = GetArcCenter(x1, y1, x2, y2, fa, fs, rx, ry, sinPhi, cosPhi);

            var theta1 = cc[2];
            var deltaTheta = cc[3];

            // Split an arc to multiple segments, so each segment
            // will be less than τ/4 (= 90°)
            var segments = MathF.Max(MathF.Ceiling(MathF.Abs(deltaTheta) / (Tau / 4)), 1);
            deltaTheta /= segments;

            // resize result
            result = new float[(int)segments][];

            for (var i = 0; i < segments; i++)
            {
                result[i] = ApproximateUnitArc(theta1, deltaTheta);
                theta1 += deltaTheta;
            }

            // We have a bezier approximation of a unit circle,
            // now need to transform back to the original ellipse
            foreach (var curve in result)
            {
                for (var i = 0; i < curve.Length; i += 2)
                {
                    var x = curve[i + 0];
                    var y = curve[i + 1];

                    // scale
                    x *= rx;
                    y *= ry;

                    // rotate
                    var xp = cosPhi * x - sinPhi * y;
                    var yp = sinPhi * x + cosPhi * y;

                    // translate
                    curve[i + 0] = xp + cc[0];
                    curve[i + 1] = yp + cc[1];
                }
            }
            
            return result;
        }

        // Calculate an angle between two unit vectors
        //
        // Since we measure angle between radii of circular arcs,
        // we can use simplified math (without length normalization)
        static float UnitVectorAngle(float ux, float uy, float vx, float vy)
        {
            var sign = ux * vy - uy * vx < 0 ? -1 : 1;
            var dot = ux * vx + uy * vy;

            // Add this to work with arbitrary vectors:
            // dot /= MathF.Sqrt(ux * ux + uy * uy) * MathF.Sqrt(vx * vx + vy * vy);

            // rounding errors, e.g. -1.0000000000000002 can screw up this
            // todo: clamp
            if (dot > 1.0)
            {
                dot = 1.0f;
            }

            if (dot < -1.0)
            {
                dot = -1.0f;
            }

            return sign * MathF.Acos(dot);
        }

        // Convert from endpoint to center parameterization,
        // see http://www.w3.org/TR/SVG11/implnote.html#ArcImplementationNotes
        //
        // Return [cx, cy, theta1, delta_theta]
        static float[] GetArcCenter(float x1, float y1, float x2, float y2, float fa, float fs,
            float rx, float ry, float sinPhi, float cosPhi)
        {
            // Step 1.
            //
            // Moving an ellipse so origin will be the middlepoint between our two
            // points. After that, rotate it to line up ellipse axes with coordinate
            // axes.
            var x1P = cosPhi * (x1 - x2) / 2 + sinPhi * (y1 - y2) / 2;
            var y1P = -sinPhi * (x1 - x2) / 2 + cosPhi * (y1 - y2) / 2;

            var rxSq = rx * rx;
            var rySq = ry * ry;
            var x1PSq = x1P * x1P;
            var y1PSq = y1P * y1P;

            // Step 2.
            //
            // Compute coordinates of the centre of this ellipse (cx', cy')
            // in the new coordinate system.
            var radicant = rxSq * rySq - rxSq * y1PSq - rySq * x1PSq;

            if (radicant < 0)
            {
                // due to rounding errors it might be e.g. -1.3877787807814457e-17
                radicant = 0;
            }

            radicant /= rxSq * y1PSq + rySq * x1PSq;
            radicant = MathF.Sqrt(radicant) * (fa == fs ? -1 : 1);

            var cxp = radicant * rx / ry * y1P;
            var cyp = radicant * -ry / rx * x1P;

            // Step 3.
            //
            // Transform back to get centre coordinates (cx, cy) in the original
            // coordinate system.
            var cx = cosPhi * cxp - sinPhi * cyp + (x1 + x2) / 2;
            var cy = sinPhi * cxp + cosPhi * cyp + (y1 + y2) / 2;

            // Step 4.
            //
            // Compute angles (theta1, delta_theta).
            var v1X = (x1P - cxp) / rx;
            var v1Y = (y1P - cyp) / ry;
            var v2X = (-x1P - cxp) / rx;
            var v2Y = (-y1P - cyp) / ry;

            var theta1 = UnitVectorAngle(1, 0, v1X, v1Y);
            var deltaTheta = UnitVectorAngle(v1X, v1Y, v2X, v2Y);

            if (fs == 0 && deltaTheta > 0)
            {
                deltaTheta -= Tau;
            }

            if (fs == 1 && deltaTheta < 0)
            {
                deltaTheta += Tau;
            }

            return [ cx, cy, theta1, deltaTheta ];
        }

        // Approximate one unit arc segment with bézier curves,
        // see http://math.stackexchange.com/questions/873224
        static float[] ApproximateUnitArc(float theta1, float deltaTheta)
        {
            var alpha = 4.0f / 3.0f * MathF.Tan(deltaTheta / 4.0f);

            var x1 = MathF.Cos(theta1);
            var y1 = MathF.Sin(theta1);
            var x2 = MathF.Cos(theta1 + deltaTheta);
            var y2 = MathF.Sin(theta1 + deltaTheta);

            return [ x1, y1, x1 - y1 * alpha, y1 + x1 * alpha, x2 + y2 * alpha, y2 - x2 * alpha, x2, y2 ];
        }
    }
}