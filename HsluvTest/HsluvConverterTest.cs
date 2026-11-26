using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Hsluv;
using MiniJSON;

namespace HsluvTest
{
	public class HsluvConverterTest
	{
		const double Epsilon = 0.00000000001;

		static void AssertStringEquals(string expected, string actual)
		{
			if (expected != actual)
			{
				throw new Exception(string.Format("Expected: {0}, actual: {1}", expected, actual));
			}
		}

		static void AssertFloatClose(double expected, double actual)
		{
			if (Math.Abs(expected - actual) > Epsilon)
			{
				throw new Exception(string.Format("Expected: {0:R}, actual: {1:R}", expected, actual));
			}
		}

		static void AssertClose(HsluvConverter expected, HsluvConverter actual)
		{
			AssertStringEquals(expected.hex, actual.hex);
			AssertFloatClose(expected.rgb_r, actual.rgb_r);
			AssertFloatClose(expected.rgb_g, actual.rgb_g);
			AssertFloatClose(expected.rgb_b, actual.rgb_b);
			AssertFloatClose(expected.xyz_x, actual.xyz_x);
			AssertFloatClose(expected.xyz_y, actual.xyz_y);
			AssertFloatClose(expected.xyz_z, actual.xyz_z);
			AssertFloatClose(expected.luv_l, actual.luv_l);
			AssertFloatClose(expected.luv_u, actual.luv_u);
			AssertFloatClose(expected.luv_v, actual.luv_v);
			AssertFloatClose(expected.lch_l, actual.lch_l);
			AssertFloatClose(expected.lch_c, actual.lch_c);
			AssertFloatClose(expected.lch_h, actual.lch_h);
			AssertFloatClose(expected.hsluv_h, actual.hsluv_h);
			AssertFloatClose(expected.hsluv_s, actual.hsluv_s);
			AssertFloatClose(expected.hsluv_l, actual.hsluv_l);
			AssertFloatClose(expected.hpluv_h, actual.hpluv_h);
			AssertFloatClose(expected.hpluv_p, actual.hpluv_p);
			AssertFloatClose(expected.hpluv_l, actual.hpluv_l);
		}

		static double GetSample(Dictionary<string, object> sample, string space, int index)
		{
			object value = (sample[space] as IList<object>)[index];
			return value is long ? (double)(long)value : (double)value;
		}

		static void Main(string[] args)
		{
			var assembly = Assembly.GetExecutingAssembly();

			using (Stream stream = assembly.GetManifestResourceStream("JsonSnapshotRev4"))
			using (StreamReader reader = new StreamReader(stream))
			{
				var data = Json.Deserialize(reader.ReadToEnd()) as Dictionary<string, object>;
				var conv = new HsluvConverter();

				foreach (KeyValuePair<string, object> pair in data)
				{
					var s = pair.Value as Dictionary<string, object>;
					var sample = new HsluvConverter();
					sample.hex = pair.Key;
					sample.rgb_r = GetSample(s, "rgb", 0);
					sample.rgb_g = GetSample(s, "rgb", 1);
					sample.rgb_b = GetSample(s, "rgb", 2);
					sample.xyz_x = GetSample(s, "xyz", 0);
					sample.xyz_y = GetSample(s, "xyz", 1);
					sample.xyz_z = GetSample(s, "xyz", 2);
					sample.luv_l = GetSample(s, "luv", 0);
					sample.luv_u = GetSample(s, "luv", 1);
					sample.luv_v = GetSample(s, "luv", 2);
					sample.lch_l = GetSample(s, "lch", 0);
					sample.lch_c = GetSample(s, "lch", 1);
					sample.lch_h = GetSample(s, "lch", 2);
					sample.hsluv_h = GetSample(s, "hsluv", 0);
					sample.hsluv_s = GetSample(s, "hsluv", 1);
					sample.hsluv_l = GetSample(s, "hsluv", 2);
					sample.hpluv_h = GetSample(s, "hpluv", 0);
					sample.hpluv_p = GetSample(s, "hpluv", 1);
					sample.hpluv_l = GetSample(s, "hpluv", 2);

					// forward functions
					conv.hex = pair.Key;
					conv.HexToHsluv();
					AssertClose(sample, conv);
					conv.HexToHpluv();
					AssertClose(sample, conv);

					// backward functions
					conv.hsluv_h = sample.hsluv_h;
					conv.hsluv_s = sample.hsluv_s;
					conv.hsluv_l = sample.hsluv_l;
					conv.HsluvToHex();
					AssertClose(sample, conv);

					conv.hpluv_h = sample.hpluv_h;
					conv.hpluv_p = sample.hpluv_p;
					conv.hpluv_l = sample.hpluv_l;
					conv.HpluvToHex();
					AssertClose(sample, conv);
				}
			}

			Console.WriteLine("Success!");
		}
	}
}
