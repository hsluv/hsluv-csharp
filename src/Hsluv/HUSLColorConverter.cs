using System;
using System.Globalization;

namespace Hsluv
{
    public static class HUSLColorConverter
    {
        private static readonly double[][] M =
        {
            new[] { 3.2409699419045214, -1.5373831775700935, -0.49861076029300328 },
            new[] { -0.96924363628087983, 1.8759675015077207, 0.041555057407175613 },
            new[] { 0.055630079696993609, -0.20397695888897657, 1.0569715142428786 },
        };

        private static readonly double[][] MInv =
        {
            new[] { 0.41239079926595948, 0.35758433938387796, 0.18048078840183429 },
            new[] { 0.21263900587151036, 0.71516867876775593, 0.072192315360733715 },
            new[] { 0.019330818715591851, 0.11919477979462599, 0.95053215224966058 },
        };

        private const double RefU = 0.19783000664283681;
        private const double RefV = 0.468319994938791;

        private const double Kappa = 903.2962962962963;
        private const double Epsilon = 0.0088564516790356308;


        public static string HsluvToHex(double[] value)
            => HsluvToHex(value[0], value[1], value[2]);

        public static string HsluvToHex((double h, double s, double l) value)
            => HsluvToHex(value.h, value.s, value.l);

        public static string HsluvToHex(double h, double s, double l)
            => RgbToHex(HsluvToRgb(h, s, l));

        public static (double h, double p, double l) HexToHsluv(string hex)
            => RgbToHsluv(HexToRgb(hex));

        public static string HpluvToHex(double[] value)
            => HpluvToHex(value[0], value[1], value[2]);

        public static string HpluvToHex((double h, double s, double l) value)
            => HpluvToHex(value.h, value.s, value.l);

        public static string HpluvToHex(double h, double s, double l)
            => RgbToHex(XyzToRgb(LuvToXyz(LchToLuv(HpluvToLch(h, s, l)))));

        public static (double h, double s, double l) HexToHpluv(string hex)
            => LchToHpluv(LuvToLch(XyzToLuv(RgbToXyz(HexToRgb(hex)))));

        public static double[] HsluvToRgb(double[] value)
        {
            var (r, g, b) = HsluvToRgb(value[0], value[1], value[2]);
            return new[] { r, g, b };
        }

        public static (double r, double g, double b) HsluvToRgb((double h, double s, double l) value)
            => HsluvToRgb(value.h, value.s, value.l);

        public static (double r, double g, double b) HsluvToRgb(double h, double s, double l)
            => LchToRgb(HsluvToLch(h, s, l));

        public static double[] LchToRgb(double[] value)
        {
            var (r, g, b) = LchToRgb(value[0], value[1], value[2]);
            return new[] { r, g, b };
        }

        public static (double r, double g, double b) LchToRgb((double l, double c, double h) value)
            => LchToRgb(value.l, value.c, value.h);

        public static (double r, double g, double b) LchToRgb(double l, double c, double h)
            => XyzToRgb(LuvToXyz(LchToLuv(l, c, h)));

        public static double[] RgbToHsluv(double[] value)
        {
            var (h, s, l) = RgbToHsluv(value[0], value[1], value[2]);
            return new[] { h, s, l };
        }

        public static (double h, double s, double l) RgbToHsluv((double r, double g, double b) value)
            => RgbToHsluv(value.r, value.g, value.b);

        public static (double h, double s, double l) RgbToHsluv(double r, double g, double b)
            => LchToHsluv(RgbToLch(r, g, b));

        public static double[] RgbToLch(double[] value)
        {
            var (l, c, h) = RgbToLch(value[0], value[1], value[2]);
            return new[] { l, c, h };
        }

        public static (double l, double c, double h) RgbToLch((double r, double g, double b) value)
            => RgbToLch(value.r, value.g, value.b);

        public static (double l, double c, double h) RgbToLch(double r, double g, double b)
            => LuvToLch(XyzToLuv(RgbToXyz(r, g, b)));

        public static double[] XyzToLuv(double[] value)
        {
            var (l, u, v) = XyzToLuv(value[0], value[1], value[2]);
            return new[] { l, u, v };
        }

        public static (double l, double u, double v) XyzToLuv((double x, double y, double z) value)
            => XyzToLuv(value.x, value.y, value.z);

        public static (double l, double u, double v) XyzToLuv(double x, double y, double z)
        {
            double l = 0, u = 0, v = 0;
            if (y == 0)
            {
                return (l, u, v);
            }

            l = YToL(y);
            var varU = (4 * x) / (x + (15 * y) + (3 * z));
            var varV = (9 * y) / (x + (15 * y) + (3 * z));
            u = 13 * l * (varU - RefU);
            v = 13 * l * (varV - RefV);

            return (l, u, v);
        }

        public static double[] LuvToXyz(double[] value)
        {
            var (x, y, z) = LuvToXyz(value[0], value[1], value[2]);
            return new[] { x, y, z };
        }

        public static (double x, double y, double z) LuvToXyz((double l, double u, double v) value)
            => LuvToXyz(value.l, value.u, value.v);

        public static (double x, double y, double z) LuvToXyz(double l, double u, double v)
        {
            double x = 0, y = 0, z = 0;
            if (l == 0)
            {
                return (x, y, z);
            }

            var varU = u / (13.0 * l) + RefU;
            var varV = v / (13.0 * l) + RefV;

            y = LToY(l);
            x = 0.0 - (9.0 * y * varU) / ((varU - 4.0) * varV - varU * varV);
            z = (9.0 * y - (15.0 * varV * y) - (varV * x)) / (3.0 * varV);

            return (x, y, z);
        }

        public static double[] LuvToLch(double[] value)
        {
            var (l, u, v) = LuvToLch(value[0], value[1], value[2]);
            return new[] { l, u, v };
        }

        public static (double l, double c, double h) LuvToLch((double l, double u, double v) value)
            => LuvToLch(value.l, value.u, value.v);

        public static (double l, double c, double h) LuvToLch(double l, double u, double v)
        {
            double hRad, h = 0;
            var c = Math.Sqrt(Math.Pow(u, 2) + Math.Pow(v, 2));
            if (c >= 0.00000001)
            {
                hRad = Math.Atan2(v, u);
                h = hRad * 360.0 / 2.0 / Math.PI;
                if (h < 0.0)
                {
                    h = 360.0 + h;
                }
            }

            return (l, c, h);
        }

        public static double[] LchToLuv(double[] value)
        {
            var (l, u, v) = LchToLuv(value[0], value[1], value[2]);
            return new[] { l, u, v };
        }

        public static (double l, double u, double v) LchToLuv((double l, double c, double h) value)
            => LchToLuv(value.l, value.c, value.h);

        public static (double l, double u, double v) LchToLuv(double l, double c, double h)
        {
            var hRad = h / 360.0 * 2.0 * Math.PI;
            var u = Math.Cos(hRad) * c;
            var v = Math.Sin(hRad) * c;
            return (l, u, v);
        }

        public static double[] HsluvToLch(double[] value)
        {
            var (l, c, h) = HsluvToLch(value[0], value[1], value[2]);
            return new[] { l, c, h };
        }

        public static (double l, double c, double h) HsluvToLch((double h, double s, double l) value)
            => HsluvToLch(value.h, value.s, value.l);

        public static (double l, double c, double h) HsluvToLch(double h, double s, double l)
        {
            double c;
            if (l > 99.9999999 || l < 0.00000001)
            {
                c = 0.0;
            }
            else
            {
                var max = MaxChromaForLH(l, h);
                c = max / 100.0 * s;
            }

            return (l, c, h);
        }

        public static double[] LchToHsluv(double[] value)
        {
            var (h, s, l) = LchToHsluv(value[0], value[1], value[2]);
            return new[] { h, s, l };
        }

        public static (double h, double s, double l) LchToHsluv((double l, double c, double h) value)
            => LchToHsluv(value.l, value.c, value.h);

        public static (double h, double s, double l) LchToHsluv(double l, double c, double h)
        {
            double s;
            if (l > 99.9999999 || l < 0.00000001)
            {
                s = 0.0;
            }
            else
            {
                var max = MaxChromaForLH(l, h);
                s = c / max * 100.0;
            }

            return (h, s, l);
        }

        public static double[] HpluvToLch(double[] value)
        {
            var (l, c, h) = HpluvToLch(value[0], value[1], value[2]);
            return new[] { l, c, h };
        }

        public static (double l, double h, double c) HpluvToLch((double h, double s, double l) value)
            => LchToHpluv(value.h, value.s, value.l);

        public static (double l, double h, double c) HpluvToLch(double h, double s, double l)
        {
            double c;
            if (l > 99.9999999 || l < 0.00000001)
            {
                c = 0;
            }
            else
            {
                var max = MaxSafeChromaForL(l);
                c = max / 100.0 * s;
            }

            return (l, c, h);
        }

        public static double[] LchToHpluv(double[] value)
        {
            var (h, s, l) = LchToHpluv(value[0], value[1], value[2]);
            return new[] { h, s, l };
        }

        public static (double h, double s, double l) LchToHpluv((double l, double c, double h) value)
            => LchToHpluv(value.l, value.c, value.h);

        public static (double h, double s, double l) LchToHpluv(double l, double c, double h)
        {
            double s;
            if (l > 99.9999999 || l < 0.00000001)
            {
                s = 0;
            }
            else
            {
                var max = MaxSafeChromaForL(l);
                s = c / max * 100.0;
            }

            return (h, s, l);
        }

        public static string RgbToHex(double[] value)
            => RgbToHex(value[0], value[1], value[2]);

        public static string RgbToHex((double r, double g, double b) value)
            => RgbToHex(value.r, value.g, value.b);

        public static string RgbToHex(double r, double g, double b)
        {
            var rv = Round(Math.Max(0, Math.Min(r, 1)) * 255);
            var gv = Round(Math.Max(0, Math.Min(g, 1)) * 255);
            var bv = Round(Math.Max(0, Math.Min(b, 1)) * 255);

            return $"#{rv:x2}{gv:x2}{bv:x2}";
        }


        public static (double r, double g, double b) HexToRgb(string hex)
        {
            if (hex.StartsWith("#"))
            {
                hex = hex.Substring(1);
            }

            var rv = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            var gv = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            var bv = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            return (rv / 255.0, gv / 255.0, bv / 255.0);
        }

        public static double[] XyzToRgb(double[] value)
        {
            var (r, g, b) = XyzToRgb(value[0], value[1], value[2]);
            return new[] { r, g, b };
        }

        public static (double r, double g, double b) XyzToRgb((double x, double y, double z) value)
            => XyzToRgb(value.x, value.y, value.z);

        public static (double r, double g, double b) XyzToRgb(double x, double y, double z)
        {
            var r = FromLinear(DotProduct(M[0], new[] { x, y, z }));
            var g = FromLinear(DotProduct(M[1], new[] { x, y, z }));
            var b = FromLinear(DotProduct(M[2], new[] { x, y, z }));
            return (r, g, b);
        }

        public static double[] RgbToXyz(double[] value)
        {
            var (x, y, z) = RgbToXyz(value[0], value[1], value[2]);
            return new[] { x, y, z };
        }

        public static (double x, double y, double z) RgbToXyz((double r, double g, double b) value)
            => RgbToXyz(value.r, value.g, value.b);

        public static (double x, double y, double z) RgbToXyz(double r, double g, double b)
        {
            r = ToLine(r);
            g = ToLine(g);
            b = ToLine(b);
            var x = DotProduct(MInv[0], new[] { r, g, b });
            var y = DotProduct(MInv[1], new[] { r, g, b });
            var z = DotProduct(MInv[2], new[] { r, g, b });
            return (x, y, z);
        }

        private static double FromLinear(double c)
        {
            if (c <= 0.0031308)
            {
                return 12.92 * c;
            }

            return 1.055 * Math.Pow(c, 1.0 / 2.4) - 0.055;
        }

        private static double ToLine(double c)
        {
            const double a = 0.055;
            if (c > 0.04045)
            {
                return Math.Pow((c + a) / (1.0 + a), 2.4);
            }

            return c / 12.92;
        }

        private static double YToL(double y)
        {
            if (y <= Epsilon)
            {
                return y * Kappa;
            }

            return 116.0 * Math.Pow(y, 1.0 / 3.0) - 16.0;
        }

        private static double LToY(double l)
        {
            if (l <= 8)
            {
                return l / Kappa;
            }

            return Math.Pow((l + 16) / 116, 3);
        }

        private static double MaxSafeChromaForL(double l)
        {
            var minLength = double.MaxValue;
            var lines = GetBounds(l);
            for (var i = 0; i < lines.Length; i++)
            {
                var (m1, b1) = lines[i];
                var x = IntersectLineLine(m1, b1, -1.0 / m1, 0.0);
                var dist = DistanceFromPole(x, b1 + x * m1);
                minLength = Math.Min(minLength, dist);
            }

            return minLength;
        }

        private static double MaxChromaForLH(double l, double h)
        {
            var hRad = h / 360.0 * Math.PI * 2.0;
            var minLength = double.MaxValue;

            var lines = GetBounds(l);
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var length = LengthOfRayUntilIntersect(hRad, line.Item1, line.Item2);
                if (length > 0.0)
                {
                    minLength = Math.Min(minLength, length);
                }
            }

            return minLength;
        }

        private static (double, double)[] GetBounds(double l)
        {
            var ret = new (double, double)[6];

            var sub1 = Math.Pow(l + 16, 3) / 1560896.0;

            var sub2 = sub1;
            if (sub1 <= Epsilon)
            {
                sub2 = l / Kappa;
            }

            for (var i = 0; i < M.Length; i++)
            {
                for (var k = 0; k < 2; k++)
                {
                    var top1 = (284517.0 * M[i][0] - 94839.0 * M[i][2]) * sub2;
                    var top2 = (838422.0 * M[i][2] + 769860.0 * M[i][1] + 731718.0 * M[i][0]) * l * sub2 -
                               769860.0 * k * l;
                    var bottom = (632260.0 * M[i][2] - 126452.0 * M[i][1]) * sub2 + 126452.0 * k;
                    ret[i * 2 + k] = (top1 / bottom, top2 / bottom);
                }
            }

            return ret;
        }

        private static double IntersectLineLine(double x1, double y1, double x2, double y2)
            => (y1 - y2) / (x2 - x1);

        private static double DistanceFromPole(double x, double y)
            => Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

        private static double LengthOfRayUntilIntersect(double theta, double x, double y)
            => y / (Math.Sin(theta) - x * Math.Cos(theta));

        private static double DotProduct(double[] a, double[] b)
        {
            var sum = 0.0;
            for (var i = 0; i < a.Length; i++)
            {
                sum += a[i] * b[i];
            }

            return sum;
        }

        private static int Round(double f)
        {
            if (Math.Abs(f) < 0.5)
            {
                return 0;
            }

#if NETSTANDARD
            return (int)(f + CopySign(0.5, f));
#else
            return (int)(f + Math.CopySign(0.5, f));
#endif
        }

#if NETSTANDARD
        private static double CopySign(double x, double y)
        {
            const long signMask = 1L << 63;

            // This method is required to work for all inputs,
            // including NaN, so we operate on the raw bits.
            long xbits = BitConverter.DoubleToInt64Bits(x);
            long ybits = BitConverter.DoubleToInt64Bits(y);

            // Remove the sign from x, and remove everything but the sign from y
            xbits &= ~signMask;
            ybits &= signMask;

            // Simply OR them to get the correct sign
            return BitConverter.Int64BitsToDouble(xbits | ybits);
        }
#endif
    }
}