using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace Hsluv.Tests;

public class ColorConverterTest
{
    private const double MaxDiff = 0.00000001;
    private const double MaxRelDiff = 0.000000001;

    private readonly ITestOutputHelper _output;

    public ColorConverterTest(ITestOutputHelper output)
    {
        _output = output;
    }

    private static bool AssertAlmostEqualRelativeAndAbs(double a, double b)
    {
        // Check if the numbers are really close -- needed
        // when comparing numbers near zero.
        var diff = Math.Abs(a - b);
        if (diff <= MaxDiff)
        {
            return true;
        }

        a = Math.Abs(a);
        b = Math.Abs(b);
        var largest = (b > a) ? b : a;

        return diff <= largest * MaxRelDiff;
    }

    private void AssertTuplesClose(string label, double[] expected, double[] actual)
    {
        var mismatch = false;
        var deltas = new double[expected.Length];

        for (var i = 0; i < expected.Length; ++i)
        {
            deltas[i] = Math.Abs(expected[i] - actual[i]);
            if (!AssertAlmostEqualRelativeAndAbs(expected[i], actual[i]))
            {
                mismatch = true;
            }
        }

        if (mismatch)
        {
            _output.WriteLine("MISMATCH {0}", label);
            _output.WriteLine(" expected: {0}, {1}, {2}", expected[0], expected[1], expected[2]);
            _output.WriteLine("   actual: {0}, {1}, {2}", actual[0], actual[1], actual[2]);
            _output.WriteLine("   deltas: {0}, {1}, {2}", deltas[0], deltas[1], deltas[2]);
        }

        Assert.False(mismatch);
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