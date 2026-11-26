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

# Supported platforms

The NuGet package ships two builds of the library:

- `netstandard2.0`: .NET Core 2.0+, .NET 5+, .NET Framework 4.6.1+, Mono 5.4+, Unity, Xamarin
- `net40`: .NET Framework 4.0+

The library is plain C# 7.3 with no dependencies, so `Hsluv/Hsluv.cs` can also be copied into any project directly.

# Building and testing

Requires the [.NET SDK](https://dotnet.microsoft.com/download) (8.0 and 10.0 to run all test targets).

    dotnet build -c Release
    dotnet run -c Release --project HsluvTest -f net8.0
    dotnet run -c Release --project HsluvTest -f net10.0
    dotnet run -c Release --project HsluvTest -f net48   # Windows, or Mono elsewhere

CI also compiles the sources with Mono's `mcs`, see `.github/workflows/test.yml`.

# Packaging

To build the package locally:

    dotnet pack Hsluv -c Release -o dist

# Releasing

Releases are published to [NuGet](https://www.nuget.org/packages/Hsluv) by the
[`release.yml`](.github/workflows/release.yml) workflow, triggered by a `v*` tag. It uses
[NuGet trusted publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing),
so no long-lived API key is stored in GitHub.

One-time setup:

1. On nuget.org, sign in as an owner of the `Hsluv` package, open **Trusted Publishing** and add a policy:
   - Package Owner: your account (it must be an owner of `Hsluv`, see **Manage → Owners** on the package page)
   - Repository Owner: `hsluv` (the GitHub organization)
   - Repository: `hsluv-csharp`
   - Workflow File: `release.yml`
   - Environment: `release`
   - Scopes: select the `Hsluv` package (or use glob pattern `Hsluv`)
2. Store the nuget.org username (profile name, not email) of the policy owner as a repo secret:

   ```bash
   gh secret set NUGET_USER --repo hsluv/hsluv-csharp --body "<nuget.org username>"
   ```

Then bump `<Version>` in `Hsluv/Hsluv.csproj` and create a release:

```bash
gh release create v2.0.0 --generate-notes
```

# Authors

Mark Wonnacott, Alexei Boronine
