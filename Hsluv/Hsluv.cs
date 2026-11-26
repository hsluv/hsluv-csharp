using System;

namespace Hsluv
{
	/// <summary>
	/// Human-friendly HSL conversion utility class.
	///
	/// This is a port of the HSLuv reference implementation (revision 4):
	/// https://github.com/hsluv/hsluv-haxe
	///
	/// The math for most of this module was taken from:
	///
	/// * http://www.easyrgb.com
	/// * http://www.brucelindbloom.com
	/// * Wikipedia
	///
	/// All numbers below taken from math/bounds.wxm wxMaxima file. We use 17
	/// digits of decimal precision to export the numbers, effectively exporting
	/// them as double precision IEEE 754 floats.
	///
	/// "If an IEEE 754 double precision is converted to a decimal string with at
	/// least 17 significant digits and then converted back to double, then the
	/// final number must match the original"
	///
	/// Source: https://en.wikipedia.org/wiki/Double-precision_floating-point_format
	///
	/// This class is designed to avoid heap allocation.
	/// </summary>
	public class HsluvConverter
	{
		// RGB
		public string hex;
		public double rgb_r;
		public double rgb_g;
		public double rgb_b;

		// CIE XYZ
		public double xyz_x;
		public double xyz_y;
		public double xyz_z;

		// CIE LUV
		public double luv_l;
		public double luv_u;
		public double luv_v;

		// CIE LUV LCh
		public double lch_l;
		public double lch_c;
		public double lch_h;

		// HSLuv
		public double hsluv_h;
		public double hsluv_s;
		public double hsluv_l;

		// HPLuv
		public double hpluv_h;
		public double hpluv_p;
		public double hpluv_l;

		// 6 lines in slope-intercept format: R < 0, R > 1, G < 0, G > 1, B < 0, B > 1
		public double r0s;
		public double r0i;
		public double r1s;
		public double r1i;

		public double g0s;
		public double g0i;
		public double g1s;
		public double g1i;

		public double b0s;
		public double b0i;
		public double b1s;
		public double b1i;

		private const string hexChars = "0123456789abcdef";

		private const double refY = 1.0;

		private const double refU = 0.19783000664283;
		private const double refV = 0.46831999493879;

		// CIE LUV constants
		private const double kappa = 903.2962962;
		private const double epsilon = 0.0088564516;

		// Transformation matrix
		private const double m_r0 = 3.240969941904521;
		private const double m_r1 = -1.537383177570093;
		private const double m_r2 = -0.498610760293;

		private const double m_g0 = -0.96924363628087;
		private const double m_g1 = 1.87596750150772;
		private const double m_g2 = 0.041555057407175;

		private const double m_b0 = 0.055630079696993;
		private const double m_b1 = -0.20397695888897;
		private const double m_b2 = 1.056971514242878;

		// Used for rgb conversions
		private static double FromLinear(double c)
		{
			if (c <= 0.0031308)
			{
				return 12.92 * c;
			}
			else
			{
				return 1.055 * Math.Pow(c, 1 / 2.4) - 0.055;
			}
		}

		private static double ToLinear(double c)
		{
			if (c > 0.04045)
			{
				return Math.Pow((c + 0.055) / (1 + 0.055), 2.4);
			}
			else
			{
				return c / 12.92;
			}
		}

		// http://en.wikipedia.org/wiki/CIELUV
		// In these formulas, Yn refers to the reference white point. We are using
		// illuminant D65, so Yn (see refY in Maxima file) equals 1. The formula is
		// simplified accordingly.
		public static double YToL(double Y)
		{
			if (Y <= epsilon)
			{
				return (Y / refY) * kappa;
			}
			else
			{
				return 116 * Math.Pow(Y / refY, 1.0 / 3.0) - 16;
			}
		}

		public static double LToY(double L)
		{
			if (L <= 8)
			{
				return refY * L / kappa;
			}
			else
			{
				return refY * Math.Pow((L + 16) / 116, 3);
			}
		}

		private static string RgbChannelToHex(double chan)
		{
			// Equivalent of Haxe's Math.round (round half up)
			int c = (int)Math.Floor(chan * 255 + 0.5);
			int digit2 = c % 16;
			int digit1 = (c - digit2) / 16;
			return hexChars[digit1].ToString() + hexChars[digit2].ToString();
		}

		private static double HexToRgbChannel(string hex, int offset)
		{
			int digit1 = hexChars.IndexOf(hex[offset]);
			int digit2 = hexChars.IndexOf(hex[offset + 1]);
			int n = digit1 * 16 + digit2;
			return n / 255.0;
		}

		public void RgbToHex()
		{
			this.hex = "#"
				+ RgbChannelToHex(this.rgb_r)
				+ RgbChannelToHex(this.rgb_g)
				+ RgbChannelToHex(this.rgb_b);
		}

		public void HexToRgb()
		{
			this.hex = this.hex.ToLowerInvariant();
			this.rgb_r = HexToRgbChannel(this.hex, 1);
			this.rgb_g = HexToRgbChannel(this.hex, 3);
			this.rgb_b = HexToRgbChannel(this.hex, 5);
		}

		public void XyzToRgb()
		{
			this.rgb_r = FromLinear(m_r0 * this.xyz_x + m_r1 * this.xyz_y + m_r2 * this.xyz_z);
			this.rgb_g = FromLinear(m_g0 * this.xyz_x + m_g1 * this.xyz_y + m_g2 * this.xyz_z);
			this.rgb_b = FromLinear(m_b0 * this.xyz_x + m_b1 * this.xyz_y + m_b2 * this.xyz_z);
		}

		public void RgbToXyz()
		{
			double lr = ToLinear(this.rgb_r);
			double lg = ToLinear(this.rgb_g);
			double lb = ToLinear(this.rgb_b);
			this.xyz_x = 0.41239079926595 * lr + 0.35758433938387 * lg + 0.18048078840183 * lb;
			this.xyz_y = 0.21263900587151 * lr + 0.71516867876775 * lg + 0.072192315360733 * lb;
			this.xyz_z = 0.019330818715591 * lr + 0.11919477979462 * lg + 0.95053215224966 * lb;
		}

		public void XyzToLuv()
		{
			double divider = this.xyz_x + (15 * this.xyz_y) + (3 * this.xyz_z);
			double varU = 4 * this.xyz_x;
			double varV = 9 * this.xyz_y;

			if (divider != 0)
			{
				varU /= divider;
				varV /= divider;
			}
			else
			{
				varU = double.NaN;
				varV = double.NaN;
			}

			this.luv_l = YToL(this.xyz_y);
			if (this.luv_l == 0)
			{
				this.luv_u = 0;
				this.luv_v = 0;
			}
			else
			{
				this.luv_u = 13 * this.luv_l * (varU - refU);
				this.luv_v = 13 * this.luv_l * (varV - refV);
			}
		}

		public void LuvToXyz()
		{
			if (this.luv_l == 0)
			{
				this.xyz_x = 0;
				this.xyz_y = 0;
				this.xyz_z = 0;
				return;
			}

			double varU = this.luv_u / (13 * this.luv_l) + refU;
			double varV = this.luv_v / (13 * this.luv_l) + refV;

			this.xyz_y = LToY(this.luv_l);
			this.xyz_x = 0 - (9 * this.xyz_y * varU) / ((varU - 4) * varV - varU * varV);
			this.xyz_z = (9 * this.xyz_y - (15 * varV * this.xyz_y) - (varV * this.xyz_x)) / (3 * varV);
		}

		public void LuvToLch()
		{
			this.lch_l = this.luv_l;
			this.lch_c = Math.Sqrt(this.luv_u * this.luv_u + this.luv_v * this.luv_v);

			// Greys: disambiguate hue
			if (this.lch_c < 0.00000001)
			{
				this.lch_h = 0;
			}
			else
			{
				double Hrad = Math.Atan2(this.luv_v, this.luv_u);
				this.lch_h = (Hrad * 180.0) / Math.PI;

				if (this.lch_h < 0)
				{
					this.lch_h = 360 + this.lch_h;
				}
			}
		}

		public void LchToLuv()
		{
			double Hrad = this.lch_h / 180.0 * Math.PI;
			this.luv_l = this.lch_l;
			this.luv_u = Math.Cos(Hrad) * this.lch_c;
			this.luv_v = Math.Sin(Hrad) * this.lch_c;
		}

		/// <summary>
		/// For a given lightness, generate a list of 6 lines in slope-intercept
		/// form that represent the bounds in CIELUV, stepping over which will
		/// push a value out of the RGB gamut.
		/// </summary>
		public void CalculateBoundingLines(double l)
		{
			double sub1 = Math.Pow(l + 16, 3) / 1560896;
			double sub2 = sub1 > epsilon ? sub1 : l / kappa;

			double s1r = sub2 * (284517 * m_r0 - 94839 * m_r2);
			double s2r = sub2 * (838422 * m_r2 + 769860 * m_r1 + 731718 * m_r0);
			double s3r = sub2 * (632260 * m_r2 - 126452 * m_r1);

			double s1g = sub2 * (284517 * m_g0 - 94839 * m_g2);
			double s2g = sub2 * (838422 * m_g2 + 769860 * m_g1 + 731718 * m_g0);
			double s3g = sub2 * (632260 * m_g2 - 126452 * m_g1);

			double s1b = sub2 * (284517 * m_b0 - 94839 * m_b2);
			double s2b = sub2 * (838422 * m_b2 + 769860 * m_b1 + 731718 * m_b0);
			double s3b = sub2 * (632260 * m_b2 - 126452 * m_b1);

			this.r0s = s1r / s3r;
			this.r0i = s2r * l / s3r;
			this.r1s = s1r / (s3r + 126452);
			this.r1i = (s2r - 769860) * l / (s3r + 126452);

			this.g0s = s1g / s3g;
			this.g0i = s2g * l / s3g;
			this.g1s = s1g / (s3g + 126452);
			this.g1i = (s2g - 769860) * l / (s3g + 126452);

			this.b0s = s1b / s3b;
			this.b0i = s2b * l / s3b;
			this.b1s = s1b / (s3b + 126452);
			this.b1i = (s2b - 769860) * l / (s3b + 126452);
		}

		// theta  -- angle of ray starting at (0, 0)
		// m, b   -- slope and intercept of line
		// x1, y1 -- coordinates of intersection
		// len    -- length of ray until it intersects with line
		//
		// b + m * x1        = y1
		// len              >= 0
		// len * cos(theta)  = x1
		// len * sin(theta)  = y1
		//
		//
		// b + m * (len * cos(theta)) = len * sin(theta)
		// b = len * sin(hrad) - m * len * cos(theta)
		// b = len * (sin(hrad) - m * cos(hrad))
		// len = b / (sin(hrad) - m * cos(hrad))
		private static double DistanceFromOriginAngle(double slope, double intercept, double angle)
		{
			double d = intercept / (Math.Sin(angle) - slope * Math.Cos(angle));
			if (d < 0)
			{
				return double.PositiveInfinity;
			}
			else
			{
				return d;
			}
		}

		private static double DistanceFromOrigin(double slope, double intercept)
		{
			return Math.Abs(intercept) / Math.Sqrt(Math.Pow(slope, 2) + 1);
		}

		private static double Min6(double f1, double f2, double f3, double f4, double f5, double f6)
		{
			return Math.Min(f1, Math.Min(f2, Math.Min(f3, Math.Min(f4, Math.Min(f5, f6)))));
		}

		/// <summary>
		/// For HPLuv, we simply see which bounding line is closest to the origin
		/// by measuring the length of its perpendicular. The shortest perpendicular
		/// determines maximum chroma.
		/// </summary>
		public double CalcMaxChromaHpluv()
		{
			double r0 = DistanceFromOrigin(this.r0s, this.r0i);
			double r1 = DistanceFromOrigin(this.r1s, this.r1i);
			double g0 = DistanceFromOrigin(this.g0s, this.g0i);
			double g1 = DistanceFromOrigin(this.g1s, this.g1i);
			double b0 = DistanceFromOrigin(this.b0s, this.b0i);
			double b1 = DistanceFromOrigin(this.b1s, this.b1i);
			return Min6(r0, r1, g0, g1, b0, b1);
		}

		/// <summary>
		/// For HSLuv, we draw a ray from origin with angle specified by the hue.
		/// Multiple bounding lines may intercept this ray. The first one to intercept
		/// it determines maximum chroma.
		/// </summary>
		public double CalcMaxChromaHsluv(double h)
		{
			double hueRad = h / 360 * Math.PI * 2;
			double r0 = DistanceFromOriginAngle(this.r0s, this.r0i, hueRad);
			double r1 = DistanceFromOriginAngle(this.r1s, this.r1i, hueRad);
			double g0 = DistanceFromOriginAngle(this.g0s, this.g0i, hueRad);
			double g1 = DistanceFromOriginAngle(this.g1s, this.g1i, hueRad);
			double b0 = DistanceFromOriginAngle(this.b0s, this.b0i, hueRad);
			double b1 = DistanceFromOriginAngle(this.b1s, this.b1i, hueRad);
			return Min6(r0, r1, g0, g1, b0, b1);
		}

		public void HsluvToLch()
		{
			// White and black: disambiguate chroma
			if (this.hsluv_l > 99.9999999)
			{
				this.lch_l = 100;
				this.lch_c = 0;
			}
			else if (this.hsluv_l < 0.00000001)
			{
				this.lch_l = 0;
				this.lch_c = 0;
			}
			else
			{
				this.lch_l = this.hsluv_l;
				this.CalculateBoundingLines(this.hsluv_l);
				double max = this.CalcMaxChromaHsluv(this.hsluv_h);
				this.lch_c = max / 100 * this.hsluv_s;
			}
			this.lch_h = this.hsluv_h;
		}

		public void LchToHsluv()
		{
			// White and black: disambiguate chroma
			if (this.lch_l > 99.9999999)
			{
				this.hsluv_s = 0;
				this.hsluv_l = 100;
			}
			else if (this.lch_l < 0.00000001)
			{
				this.hsluv_s = 0;
				this.hsluv_l = 0;
			}
			else
			{
				this.CalculateBoundingLines(this.lch_l);
				double max = this.CalcMaxChromaHsluv(this.lch_h);
				this.hsluv_s = this.lch_c / max * 100;
				this.hsluv_l = this.lch_l;
			}
			this.hsluv_h = this.lch_h;
		}

		public void HpluvToLch()
		{
			// White and black: disambiguate chroma
			if (this.hpluv_l > 99.9999999)
			{
				this.lch_l = 100;
				this.lch_c = 0;
			}
			else if (this.hpluv_l < 0.00000001)
			{
				this.lch_l = 0;
				this.lch_c = 0;
			}
			else
			{
				this.lch_l = this.hpluv_l;
				this.CalculateBoundingLines(this.hpluv_l);
				double max = this.CalcMaxChromaHpluv();
				this.lch_c = max / 100 * this.hpluv_p;
			}
			this.lch_h = this.hpluv_h;
		}

		public void LchToHpluv()
		{
			// White and black: disambiguate chroma
			if (this.lch_l > 99.9999999)
			{
				this.hpluv_p = 0;
				this.hpluv_l = 100;
			}
			else if (this.lch_l < 0.00000001)
			{
				this.hpluv_p = 0;
				this.hpluv_l = 0;
			}
			else
			{
				this.CalculateBoundingLines(this.lch_l);
				double max = this.CalcMaxChromaHpluv();
				this.hpluv_p = this.lch_c / max * 100;
				this.hpluv_l = this.lch_l;
			}
			this.hpluv_h = this.lch_h;
		}

		public void HsluvToRgb()
		{
			this.HsluvToLch();
			this.LchToLuv();
			this.LuvToXyz();
			this.XyzToRgb();
		}

		public void HpluvToRgb()
		{
			this.HpluvToLch();
			this.LchToLuv();
			this.LuvToXyz();
			this.XyzToRgb();
		}

		public void HsluvToHex()
		{
			this.HsluvToRgb();
			this.RgbToHex();
		}

		public void HpluvToHex()
		{
			this.HpluvToRgb();
			this.RgbToHex();
		}

		public void RgbToHsluv()
		{
			this.RgbToXyz();
			this.XyzToLuv();
			this.LuvToLch();
			this.LchToHpluv();
			this.LchToHsluv();
		}

		public void RgbToHpluv()
		{
			this.RgbToXyz();
			this.XyzToLuv();
			this.LuvToLch();
			this.LchToHpluv();
			this.LchToHpluv();
		}

		public void HexToHsluv()
		{
			this.HexToRgb();
			this.RgbToHsluv();
		}

		public void HexToHpluv()
		{
			this.HexToRgb();
			this.RgbToHpluv();
		}
	}
}
