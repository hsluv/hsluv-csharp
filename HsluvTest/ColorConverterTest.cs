using System;
using System.Net;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using HsluvTest;
using Hsluv;
using NUnit.Framework;
using NUnit.Core;
using MiniJSON;

namespace HsluvTest
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
		public void HsluvTest()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = "HsluvTest.Resources.JsonSnapshotRev3.txt";

			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			using (StreamReader reader = new StreamReader(stream))
			{

				var data = Json.Deserialize(reader.ReadToEnd ()) as Dictionary<string, object>;
				foreach (KeyValuePair<string, object> pair in data)
				{
					var expected = pair.Value as Dictionary<string, object>;

					// test forward functions
					var test_rgb = ColorConverter.HexToRgb(pair.Key);
					Assert.AreEqual (expected, pair.Value);
					AssertTuplesClose(test_rgb, Cast(expected["rgb"]));
					var test_xyz = ColorConverter.RgbToXyz(test_rgb);
					AssertTuplesClose(test_xyz, Cast(expected["xyz"]));
					var test_luv = ColorConverter.XyzToLuv(test_xyz);
					AssertTuplesClose(test_luv, Cast(expected["luv"]));
					var test_lch = ColorConverter.LuvToLch(test_luv);
					AssertTuplesClose(test_lch, Cast(expected["lch"]));
					var test_hsluv = ColorConverter.LchToHsluv(test_lch);
					AssertTuplesClose(test_hsluv, Cast(expected["hsluv"]));
					var test_hpluv = ColorConverter.LchToHpluv(test_lch);
					AssertTuplesClose(test_hpluv, Cast(expected["hpluv"]));

					// test backward functions
					test_lch = ColorConverter.HsluvToLch(Cast(expected["hsluv"]));
					AssertTuplesClose(test_lch, Cast(expected["lch"]));
					test_lch = ColorConverter.HpluvToLch(Cast(expected["hpluv"]));
					AssertTuplesClose(test_lch, Cast(expected["lch"]));
					test_luv = ColorConverter.LchToLuv(test_lch);
					AssertTuplesClose(test_luv, Cast(expected["luv"]));
					test_xyz = ColorConverter.LuvToXyz(Cast(expected["luv"]));
					AssertTuplesClose(test_xyz, Cast(expected["xyz"]));
					test_rgb = ColorConverter.XyzToRgb(Cast(expected["xyz"]));
					AssertTuplesClose(test_rgb, Cast(expected["rgb"]));
					Assert.AreEqual(ColorConverter.RgbToHex(test_rgb), pair.Key);

					// full test
					Assert.AreEqual(ColorConverter.HsluvToHex(Cast(expected["hsluv"])), pair.Key);
					Assert.AreEqual(ColorConverter.HpluvToHex(Cast(expected["hpluv"])), pair.Key);

					AssertTuplesClose(Cast(expected["hsluv"]), ColorConverter.HexToHsluv(pair.Key));
					AssertTuplesClose(Cast(expected["hpluv"]), ColorConverter.HexToHpluv(pair.Key));
				}
			}
		}
	}
}

