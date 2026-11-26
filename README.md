[![Build Status](https://github.com/hsluv/hsluv-csharp/actions/workflows/test.yml/badge.svg)](https://github.com/hsluv/hsluv-csharp/actions/workflows/test.yml)
[![Package Version](https://img.shields.io/nuget/v/Hsluv.svg)](https://www.nuget.org/packages/Hsluv)

[Explanation, demo, ports etc.](http://www.hsluv.org)

# API

This library implements [revision 4](https://github.com/hsluv/hsluv-haxe) of the HSLuv
color math, ported from the Haxe reference implementation.

The API is designed to avoid heap allocation. The `Hsluv.HsluvConverter` class defines
the following public fields:

- RGB: `hex`, `rgb_r` [0;1], `rgb_g` [0;1], `rgb_b` [0;1]
- CIE XYZ: `xyz_x`, `xyz_y`, `xyz_z`
- CIE LUV: `luv_l`, `luv_u`, `luv_v`
- CIE LUV LCh: `lch_l`, `lch_c`, `lch_h`
- HSLuv: `hsluv_h` [0;360], `hsluv_s` [0;100], `hsluv_l` [0;100]
- HPLuv: `hpluv_h` [0;360], `hpluv_p` [0;100], `hpluv_l` [0;100]

To convert between color spaces, simply set the fields of the source color space, run the
conversion methods, then read the fields of the target color space.

Use the following methods to convert to and from RGB:

- HSLuv: `HsluvToRgb()`, `HsluvToHex()`, `RgbToHsluv()`, `HexToHsluv()`
- HPLuv: `HpluvToRgb()`, `HpluvToHex()`, `RgbToHpluv()`, `HexToHpluv()`

Use the following methods to do step-by-step conversion:

- Forward: `HsluvToLch()` (or `HpluvToLch()`), `LchToLuv()`, `LuvToXyz()`, `XyzToRgb()`, `RgbToHex()`
- Backward: `HexToRgb()`, `RgbToXyz()`, `XyzToLuv()`, `LuvToLch()`, `LchToHsluv()` (or `LchToHpluv()`)

For advanced usage, we also export the [bounding lines](https://www.hsluv.org/math/) in slope-intercept
format, two for each RGB channel representing the limit of the gamut. They are populated by
`CalculateBoundingLines(l)`.

- R < 0: `r0s`, `r0i`
- R > 1: `r1s`, `r1i`
- G < 0: `g0s`, `g0i`
- G > 1: `g1s`, `g1i`
- B < 0: `b0s`, `b0i`
- B > 1: `b1s`, `b1i`

Example:

```csharp
var conv = new Hsluv.HsluvConverter();
conv.hsluv_h = 10;
conv.hsluv_s = 75;
conv.hsluv_l = 65;
conv.HsluvToHex();
Console.WriteLine(conv.hex); // Will print "#ec7d82"
```

# Building

    mcs -target:library Hsluv/Hsluv.cs

# Testing

See `.github/workflows/test.yml`.

# Packaging

    $ cd Hsluv
    $ vim Hsluv.nuspec
    $ nuget pack Hsluv.nuspec

# Authors

Mark Wonnacott, Alexei Boronine
