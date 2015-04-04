using System;
using System.Net;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using HUSLTest;
using HUSL;
using NUnit.Framework;
using NUnit.Core;
using MiniJSON;

namespace HUSLTest
{
	[TestFixture]
	public class ColorConverterTest
	{

		protected void AssertTuplesClose(IList<double> a, IList<double> b)
		{
			bool mismatch = false;

			for (int i = 0; i < a.Count; ++i)
			{
				if (Math.Abs(a[i] - b[i]) >= 0.00000001)
				{
					mismatch = true;
				}
			}

			if (mismatch)
			{
				throw new AssertionException(string.Format("{0},{1},{2} vs {3},{4},{5}", a[0], a[1], a[2], b[0], b[1], b[2]));
			}
		}

		protected IList<double> Cast(object o)
		{
			var tuple = new List<double>();

			foreach (object value in (o as IList<object>))
			{
				double bv;

				if (value.GetType() == typeof(Int64))
				{
					bv = (double) ((Int64) value);
				}
				else
				{
					bv = (double) value;
				}

				tuple.Add(bv);
			}

			return tuple;
		}

		[Test]
		public void HUSLTest()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = "HUSLTest.Resources.JSONSnapshotRev3.txt";

			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			using (StreamReader reader = new StreamReader(stream))
			{

				var data = Json.Deserialize(reader.ReadToEnd ()) as Dictionary<string, object>;
				foreach (KeyValuePair<string, object> pair in data)
				{
					var expected = pair.Value as Dictionary<string, object>;

					// test forward functions
					var test_rgb = ColorConverter.HexToRGB(pair.Key);
					Assert.AreEqual (expected, pair.Value);
					AssertTuplesClose(test_rgb, Cast(expected["rgb"]));
					var test_xyz = ColorConverter.RGBToXYZ(test_rgb);
					AssertTuplesClose(test_xyz, Cast(expected["xyz"]));
					var test_luv = ColorConverter.XYZToLUV(test_xyz);
					AssertTuplesClose(test_luv, Cast(expected["luv"]));
					var test_lch = ColorConverter.LUVToLCH(test_luv);
					AssertTuplesClose(test_lch, Cast(expected["lch"]));
					var test_husl = ColorConverter.LCHToHUSL(test_lch);
					AssertTuplesClose(test_husl, Cast(expected["husl"]));
					var test_huslp = ColorConverter.LCHToHUSLP(test_lch);
					AssertTuplesClose(test_huslp, Cast(expected["huslp"]));

					// test backward functions
					test_lch = ColorConverter.HUSLToLCH(Cast(expected["husl"]));
					AssertTuplesClose(test_lch, Cast(expected["lch"]));
					test_lch = ColorConverter.HUSLPToLCH(Cast(expected["huslp"]));
					AssertTuplesClose(test_lch, Cast(expected["lch"]));
					test_luv = ColorConverter.LCHToLUV(test_lch);
					AssertTuplesClose(test_luv, Cast(expected["luv"]));
					test_xyz = ColorConverter.LUVToXYZ(Cast(expected["luv"]));
					AssertTuplesClose(test_xyz, Cast(expected["xyz"]));
					test_rgb = ColorConverter.XYZToRGB(Cast(expected["xyz"]));
					AssertTuplesClose(test_rgb, Cast(expected["rgb"]));
					Assert.AreEqual(ColorConverter.RGBToHex(test_rgb), pair.Key);

					// full test
					//Assert(HUSL.HUSLToHex(Cast(expected["husl"])) == pair.Key);
					//assert_tuples_close({husl.hex_to_husl(hex_color)}, colors.husl)
					//Assert(HUSL.HUSLPToHex() == pair.Key);
					//assert_tuples_close({husl.hex_to_huslp(hex_color)}, colors.huslp)
				}
			}
		}
	}
}

