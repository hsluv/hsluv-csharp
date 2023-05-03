[![Build Status](https://github.com/hsluv/hsluv-csharp/actions/workflows/test.yml/badge.svg)](https://github.com/hsluv/hsluv-csharp/actions/workflows/test.yml)
[![Package Version](https://img.shields.io/nuget/v/Hsluv.svg)](https://www.nuget.org/packages/Hsluv)

[Explanation, demo, ports etc.](http://www.hsluv.org)

# API

This library provides the `Hsluv` namespace with `HsluvConverter` class with
the following static methods. Tuples are three items each: R, G, B and H, S, L.

```csharp
double[] HsluvToRgb(double[] tuple)
double[] RgbToHsluv(double[] tuple)
double[] HpluvToRgb(double[] tuple)
double[] RgbToHpluv(double[] tuple)

string HsluvToHex(double[] tuple)
string HpluvToHex(double[] tuple)
double[] HexToHsluv(string s)
double[] HexToHpluv(string s)
```

# Building

```bash
dotnet build
```

# Testing

See `.github/workflows/test.yml`.

# Packaging

```bash
dotnet pack -c Release
```

# Authors

Mark Wonnacott, Alexei Boronine
