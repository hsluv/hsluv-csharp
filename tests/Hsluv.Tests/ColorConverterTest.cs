using System.Text;
using System.Text.Json;

using Xunit;

namespace Hsluv.Tests;

public class ColorConverterTest
{
    private static bool AssertAlmostEqualRelativeAndAbs(double a, double b, double tolerance)
    {
        // Check if the numbers are really close -- needed
        // when comparing numbers near zero.
        var diff = Math.Abs(a - b);

        var largest = Math.Max(Math.Abs(a), Math.Abs(b));

        return diff <= tolerance || diff <= largest * tolerance;
    }

    private static void AssertTuplesClose(string label, double[] expected, (double, double, double) actual,
        double tolerance = 0.00000001)
        => AssertTuplesClose(label, expected, new[] { actual.Item1, actual.Item2, actual.Item3 }, tolerance);

    private static void AssertTuplesClose(string label, double[] expected, double[] actual,
        double tolerance = 0.00000001)
    {
        var mismatch = false;
        var deltas = new double[expected.Length];

        for (var i = 0; i < expected.Length; ++i)
        {
            deltas[i] = Math.Abs(expected[i] - actual[i]);
            if (!AssertAlmostEqualRelativeAndAbs(expected[i], actual[i], tolerance))
            {
                mismatch = true;
            }
        }

        var sb = new StringBuilder();
        if (mismatch)
        {
            sb.Append($"MISMATCH {label} \n");
            sb.Append($" expected: {expected[0]:F18}, {expected[1]:F18}, {expected[2]:F18} \n");
            sb.Append($"   actual: {actual[0]:F18}, {actual[1]:F18}, {actual[2]:F18} \n");
            sb.Append($"   deltas: {deltas[0]:F18}, {deltas[1]:F18}, {deltas[2]:F18}");
        }

        Assert.False(mismatch, sb.ToString());
    }

    private static double[] TupleFromJsonArray(JsonElement.ArrayEnumerator array)
    {
        var result = new double[3];
        var i = 0;
        foreach (var element in array)
        {
            result[i++] = element.GetDouble();
        }

        return result;
    }

    [Fact]
    public async Task TestHsluv()
    {
        await using var fs = File.Open("snapshot-rev4.json", FileMode.Open);
        var doc = await JsonSerializer.DeserializeAsync<JsonDocument>(fs);

        foreach (var property in doc.RootElement.EnumerateObject())
        {
            var hex = property.Name;
            var expected = property.Value.EnumerateObject().ToList();
            var rgb = TupleFromJsonArray(GetArray(expected, "rgb"));
            var xyz = TupleFromJsonArray(GetArray(expected, "xyz"));
            var luv = TupleFromJsonArray(GetArray(expected, "luv"));
            var lch = TupleFromJsonArray(GetArray(expected, "lch"));
            var hsluv = TupleFromJsonArray(GetArray(expected, "hsluv"));
            var hpluv = TupleFromJsonArray(GetArray(expected, "hpluv"));

            // forward functions
            var rgbFromHex = HUSLColorConverter.HexToRgb(hex);
            var xyzFromRgb = HUSLColorConverter.RgbToXyz(rgbFromHex);
            var luvFromXyz = HUSLColorConverter.XyzToLuv(xyzFromRgb);
            var lchFromLuv = HUSLColorConverter.LuvToLch(luvFromXyz);
            var hsluvFromLch = HUSLColorConverter.LchToHsluv(lchFromLuv);
            var hpluvFromLch = HUSLColorConverter.LchToHpluv(lchFromLuv);
            var hsluvFromHex = HUSLColorConverter.HexToHsluv(hex);
            var hpluvFromHex = HUSLColorConverter.HexToHpluv(hex);

            AssertTuplesClose("hexToRgb", rgb, rgbFromHex);
            AssertTuplesClose("rgbToXyz", xyz, xyzFromRgb);
            AssertTuplesClose("xyzToLuv", luv, luvFromXyz);
            AssertTuplesClose("luvToLch", lch, lchFromLuv);
            AssertTuplesClose("lchToHsluv", hsluv, hsluvFromLch);
            AssertTuplesClose("lchToHpluv", hpluv, hpluvFromLch);
            AssertTuplesClose("hexToHsluv", hsluv, hsluvFromHex);
            AssertTuplesClose("hexToHpluv", hpluv, hpluvFromHex);

            // backward functions

            var lchFromHsluv = HUSLColorConverter.HsluvToLch(hsluv);
            var lchFromHpluv = HUSLColorConverter.HpluvToLch(hpluv);
            var luvFromLch = HUSLColorConverter.LchToLuv(lch);
            var xyzFromLuv = HUSLColorConverter.LuvToXyz(luv);
            var rgbFromXyz = HUSLColorConverter.XyzToRgb(xyz);
            var hexFromRgb = HUSLColorConverter.RgbToHex(rgb);
            var hexFromHsluv = HUSLColorConverter.HsluvToHex(hsluv);
            var hexFromHpluv = HUSLColorConverter.HpluvToHex(hpluv);

            AssertTuplesClose("hsluvToLch", lch, lchFromHsluv);
            AssertTuplesClose("hpluvToLch", lch, lchFromHpluv);
            AssertTuplesClose("lchToLuv", luv, luvFromLch);
            AssertTuplesClose("luvToXyz", xyz, xyzFromLuv);
            AssertTuplesClose("xyzToRgb", rgb, rgbFromXyz);
            Assert.Equal(hex, hexFromRgb);
            Assert.Equal(hex, hexFromHsluv);
            Assert.Equal(hex, hexFromHpluv);
        }

        static JsonElement.ArrayEnumerator GetArray(List<JsonProperty> list, string name)
            => list.First(x => x.Name == name).Value.EnumerateArray();
    }
}