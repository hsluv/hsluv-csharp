using System;
using System.Collections.Generic;

namespace Hsluv
{
    public static class HUSLColorConverter
    {
        private static readonly double[][] M =
        {
            new[] { 3.240969941904521, -1.537383177570093, -0.498610760293 },
            new[] { -0.96924363628087, 1.87596750150772, 0.041555057407175 },
            new[] { 0.055630079696993, -0.20397695888897, 1.056971514242878 },
        };

        private static readonly double[][] MInv =
        {
            new[] { 0.41239079926595, 0.35758433938387, 0.18048078840183 },
            new[] { 0.21263900587151, 0.71516867876775, 0.072192315360733 },
            new[] { 0.019330818715591, 0.11919477979462, 0.95053215224966 },
        };

        private const double RefY = 1.0;

        private const double RefU = 0.19783000664283;
        private const double RefV = 0.46831999493879;

        private const double Kappa = 903.2962962;
        private const double Epsilon = 0.0088564516;

        private static List<double[]> GetBounds(double l)
        {
            var result = new List<double[]>();

            var sub1 = Math.Pow(l + 16, 3) / 1560896;
            var sub2 = sub1 > Epsilon ? sub1 : l / Kappa;

            for (var c = 0; c < 3; c++)
            {
                var m1 = M[c][0];
                var m2 = M[c][1];
                var m3 = M[c][2];

                for (var t = 0; t < 2; t++)
                {
                    var top1 = (284517 * m1 - 94839 * m3) * sub2;
                    var top2 = (838422 * m3 + 769860 * m2 + 731718 * m1) * l * sub2 - 769860 * t * l;
                    var bottom = (632260 * m3 - 126452 * m2) * sub2 + 126452 * t;

                    result.Add(new[] { top1 / bottom, top2 / bottom });
                }
            }

            return result;
        }

        private static double IntersectLineLine(double[] lineA, double[] lineB)
            => (lineA[1] - lineB[1]) / (lineB[0] - lineA[0]);

        private static double DistanceFromPole(double[] point)
            => Math.Sqrt(Math.Pow(point[0], 2) + Math.Pow(point[1], 2));

        private static bool LengthOfRayUntilIntersect(double theta, double[] line, out double length)
        {
            length = line[1] / (Math.Sin(theta) - line[0] * Math.Cos(theta));
            return length >= 0;
        }

        private static double MaxSafeChromaForL(double l)
        {
            var bounds = GetBounds(l);
            var min = double.MaxValue;

            for (var i = 0; i < 2; i++)
            {
                var m1 = bounds[i][0];
                var b1 = bounds[i][1];
                var line = new[] { m1, b1 };

                var x = IntersectLineLine(line, new[] { -1 / m1, 0 });
                var length = DistanceFromPole(new[] { x, b1 + x * m1 });

                min = Math.Min(min, length);
            }

            return min;
        }

        private static double MaxChromaForLH(double l, double h)
        {
            var hrad = h / 360 * Math.PI * 2;

            var bounds = GetBounds(l);
            var min = double.MaxValue;

            foreach (var bound in bounds)
            {
                if (LengthOfRayUntilIntersect(hrad, bound, out double length))
                {
                    min = Math.Min(min, length);
                }
            }

            return min;
        }

        private static double DotProduct(double[] a, double[] b)
        {
            var sum = 0.0;

            for (int i = 0; i < a.Length; i++)
            {
                sum += (a[i] * b[i]);
            }

            return sum;
        }

        private static double Round(double value, int places)
        {
            var n = Math.Pow(10, places);
            return Math.Round(value * n) / n;
        }

        private static double FromLinear(double c)
        {
            if (c <= 0.0031308)
            {
                return 12.92 * c;
            }

            return 1.055 * Math.Pow(c, 1 / 2.4) - 0.055;
        }

        private static double ToLinear(double c)
        {
            if (c > 0.04045)
            {
                return Math.Pow((c + 0.055) / (1 + 0.055), 2.4);
            }

            return c / 12.92;
        }

        private static int[] RgbPrepare(double[] tuple)
        {
            for (var i = 0; i < tuple.Length; i++)
            {
                tuple[i] = Round(tuple[i], 3);
            }

            for (var i = 0; i < tuple.Length; i++)
            {
                var ch = tuple[i];
                if (ch < -0.0001 || ch > 1.0001)
                {
                    throw new ArgumentException("Illegal rgb value: " + ch, nameof(tuple));
                }
            }

            var results = new int[tuple.Length];

            for (var i = 0; i < tuple.Length; ++i)
            {
                results[i] = (int)Math.Round(tuple[i] * 255);
            }

            return results;
        }

        public static double[] XyzToRgb(double[] tuple) => new[]
        {
            FromLinear(DotProduct(M[0], tuple)), 
            FromLinear(DotProduct(M[1], tuple)),
            FromLinear(DotProduct(M[2], tuple)),
        };

        public static double[] RgbToXyz(double[] tuple)
        {
            var rgbl = new[] { ToLinear(tuple[0]), ToLinear(tuple[1]), ToLinear(tuple[2]), };
            return new[] { DotProduct(MInv[0], rgbl), DotProduct(MInv[1], rgbl), DotProduct(MInv[2], rgbl) };
        }

        private static double YToL(double y)
        {
            if (y <= Epsilon)
            {
                return (y / RefY) * Kappa;
            }

            return 116 * Math.Pow(y / RefY, 1.0 / 3.0) - 16;
        }

        private static double LToY(double l)
        {
            if (l <= 8)
            {
                return RefY * l / Kappa;
            }

            return RefY * Math.Pow((l + 16) / 116, 3);
        }

        public static double[] XyzToLuv(double[] tuple)
        {
            var X = tuple[0];
            var Y = tuple[1];
            var Z = tuple[2];

            var varU = (4 * X) / (X + (15 * Y) + (3 * Z));
            var varV = (9 * Y) / (X + (15 * Y) + (3 * Z));

            var L = YToL(Y);

            if (L == 0)
            {
                return new double[] { 0, 0, 0 };
            }

            var U = 13 * L * (varU - RefU);
            var V = 13 * L * (varV - RefV);

            return new[] { L, U, V };
        }

        public static double[] LuvToXyz(double[] tuple)
        {
            var L = tuple[0];
            var U = tuple[1];
            var V = tuple[2];

            if (L == 0)
            {
                return new double[] { 0, 0, 0 };
            }

            var varU = U / (13 * L) + RefU;
            var varV = V / (13 * L) + RefV;

            var Y = LToY(L);
            var X = 0 - (9 * Y * varU) / ((varU - 4) * varV - varU * varV);
            var Z = (9 * Y - (15 * varV * Y) - (varV * X)) / (3 * varV);

            return new[] { X, Y, Z };
        }

        public static double[] LuvToLch(double[] tuple)
        {
            var L = tuple[0];
            var U = tuple[1];
            var V = tuple[2];


            var C = Math.Sqrt(U * U + V * V);
            double H;

            if (C < 0.00000001)
            {
                H = 0.0;
            }
            else
            {
                var Hrad = Math.Atan2(V, U);
                H = Hrad * 180.0 / Math.PI;

                if (H < 0)
                {
                    H = 360 + H;
                }
            }

            return new[] { L, C, H };
        }

        public static double[] LchToLuv(double[] tuple)
        {
            var L = tuple[0];
            var C = tuple[1];
            var H = tuple[2];

            var Hrad = H / 360.0 * 2 * Math.PI;
            var U = Math.Cos(Hrad) * C;
            var V = Math.Sin(Hrad) * C;

            return new[] { L, U, V };
        }

        public static double[] HsluvToLch(double[] tuple)
        {
            var H = tuple[0];
            var S = tuple[1];
            var L = tuple[2];

            if (L > 99.9999999)
            {
                return new[] { 100, 0, H };
            }

            if (L < 0.00000001)
            {
                return new[] { 0, 0, H };
            }

            var max = MaxChromaForLH(L, H);
            var C = max / 100 * S;

            return new[] { L, C, H };
        }

        public static double[] LchToHsluv(double[] tuple)
        {
            var L = tuple[0];
            var C = tuple[1];
            var H = tuple[2];

            if (L > 99.9999999)
            {
                return new[] { H, 100, 0 };
            }

            if (L < 0.00000001)
            {
                return new[] { H, 0, 0 };
            }

            var max = MaxChromaForLH(L, H);
            var S = C / max * 100;

            return new[] { H, S, L };
        }

        public static double[] HpluvToLch(double[] tuple)
        {
            var H = tuple[0];
            var S = tuple[1];
            var L = tuple[2];

            if (L > 99.9999999)
            {
                return new[] { 100, 0, H };
            }

            if (L < 0.00000001)
            {
                return new[] { 0, 0, H };
            }

            var max = MaxSafeChromaForL(L);
            var C = max / 100 * S;

            return new[] { L, C, H };
        }

        public static double[] LchToHpluv(double[] tuple)
        {
            var L = tuple[0];
            var C = tuple[1];
            var H = tuple[2];

            if (L > 99.9999999)
            {
                return new[] { H, 0, 100 };
            }

            if (L < 0.00000001)
            {
                return new[] { H, 0, 0 };
            }

            var max = MaxSafeChromaForL(L);
            var S = C / max * 100;

            return new[] { H, S, L };
        }

        public static string RgbToHex(double[] tuple)
        {
            var prepared = RgbPrepare(tuple);
            return $"#{prepared[0]:x2}{prepared[1]:x2}{prepared[2]:x2}";
        }

        public static double[] HexToRgb(string hex) => new[]
        {
            int.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber) / 255.0,
            int.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber) / 255.0,
            int.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber) / 255.0,
        };

        public static double[] LchToRgb(double[] tuple)
            => XyzToRgb(LuvToXyz(LchToLuv(tuple)));

        public static double[] RgbToLch(double[] tuple)
            => LuvToLch(XyzToLuv(RgbToXyz(tuple)));

        // Rgb <--> Hsluv(p)

        public static double[] HsluvToRgb(double[] tuple) => LchToRgb(HsluvToLch(tuple));

        public static double[] RgbToHsluv(double[] tuple) => LchToHsluv(RgbToLch(tuple));

        public static double[] HpluvToRgb(double[] tuple) => LchToRgb(HpluvToLch(tuple));

        public static double[] RgbToHpluv(double[] tuple) => LchToHpluv(RgbToLch(tuple));

        // Hex

        public static string HsluvToHex(double[] tuple) => RgbToHex(HsluvToRgb(tuple));

        public static string HpluvToHex(double[] tuple) => RgbToHex(HpluvToRgb(tuple));

        public static double[] HexToHsluv(string s) => RgbToHsluv(HexToRgb(s));

        public static double[] HexToHpluv(string s) => RgbToHpluv(HexToRgb(s));
    }
}