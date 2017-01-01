using System;
using System.Net;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using HsluvTest;
using Hsluv;
using MiniJSON;

namespace HsluvTest
{
	public class HsluvConverterTest
	{

        static void AssertAreEqual(string a, string b)
        {
            if (!a.Equals(b)) {
                throw new Exception(string.Format("Expected: {0}, actual: {1}", a, b));
            }
        }

		static void AssertTuplesClose(IList<double> a, IList<double> b)
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
				throw new Exception(string.Format("{0},{1},{2} vs {3},{4},{5}", a[0], a[1], a[2], b[0], b[1], b[2]));
			}
		}

		static IList<double> Cast(object o)
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

		static void Main(string[] args)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = "JsonSnapshotRev3";

			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			using (StreamReader reader = new StreamReader(stream))
			{

				var data = Json.Deserialize(reader.ReadToEnd ()) as Dictionary<string, object>;
				foreach (KeyValuePair<string, object> pair in data)
				{
					var expected = pair.Value as Dictionary<string, object>;

					// test forward functions
					var test_rgb = HsluvConverter.HexToRgb(pair.Key);
					AssertTuplesClose(test_rgb, Cast(expected["rgb"]));
					var test_xyz = HsluvConverter.RgbToXyz(test_rgb);
					AssertTuplesClose(test_xyz, Cast(expected["xyz"]));
					var test_luv = HsluvConverter.XyzToLuv(test_xyz);
					AssertTuplesClose(test_luv, Cast(expected["luv"]));
					var test_lch = HsluvConverter.LuvToLch(test_luv);
					AssertTuplesClose(test_lch, Cast(expected["lch"]));
					var test_hsluv = HsluvConverter.LchToHsluv(test_lch);
					AssertTuplesClose(test_hsluv, Cast(expected["hsluv"]));
					var test_hpluv = HsluvConverter.LchToHpluv(test_lch);
					AssertTuplesClose(test_hpluv, Cast(expected["hpluv"]));

					// test backward functions
					test_lch = HsluvConverter.HsluvToLch(Cast(expected["hsluv"]));
					AssertTuplesClose(test_lch, Cast(expected["lch"]));
					test_lch = HsluvConverter.HpluvToLch(Cast(expected["hpluv"]));
					AssertTuplesClose(test_lch, Cast(expected["lch"]));
					test_luv = HsluvConverter.LchToLuv(test_lch);
					AssertTuplesClose(test_luv, Cast(expected["luv"]));
					test_xyz = HsluvConverter.LuvToXyz(Cast(expected["luv"]));
					AssertTuplesClose(test_xyz, Cast(expected["xyz"]));
					test_rgb = HsluvConverter.XyzToRgb(Cast(expected["xyz"]));
					AssertTuplesClose(test_rgb, Cast(expected["rgb"]));
					AssertAreEqual(HsluvConverter.RgbToHex(test_rgb), pair.Key);

					// full test
					AssertAreEqual(HsluvConverter.HsluvToHex(Cast(expected["hsluv"])), pair.Key);
					AssertAreEqual(HsluvConverter.HpluvToHex(Cast(expected["hpluv"])), pair.Key);
					AssertTuplesClose(Cast(expected["hsluv"]), HsluvConverter.HexToHsluv(pair.Key));
					AssertTuplesClose(Cast(expected["hpluv"]), HsluvConverter.HexToHpluv(pair.Key));
				}
			}

			Console.WriteLine("Success!");
		}
	}
}

