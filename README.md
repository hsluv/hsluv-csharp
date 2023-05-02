[![Build Status](https://travis-ci.org/hsluv/hsluv-csharp.svg?branch=master)](https://travis-ci.org/hsluv/hsluv-csharp)
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

See `.travis.yml`.

# Packaging

```bash
dotnet pack -c Release
```

# Authors

Mark Wonnacott, Alexei Boronine
