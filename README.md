[![Build Status](https://travis-ci.org/hsluv/hsluv-csharp.svg?branch=master)](https://travis-ci.org/hsluv/hsluv-csharp)
[![Package Version](https://img.shields.io/nuget/v/Hsluv.svg)](https://www.nuget.org/packages/Hsluv)

[Explanation, demo, ports etc.](http://www.hsluv.org)

# API

This library provides the `Hsluv` namespace with `HsluvConverter` class with
the following static methods. Tuples are three items each: R, G, B and H, S, L.

    IList<double> HsluvToRgb(IList<double> tuple)
	IList<double> RgbToHsluv(IList<double> tuple)
	IList<double> HpluvToRgb(IList<double> tuple)
	IList<double> RgbToHpluv(IList<double> tuple)

	string HsluvToHex(IList<double> tuple)
	string HpluvToHex(IList<double> tuple)
	IList<double> HexToHsluv(string s)
	IList<double> HexToHpluv(string s)

# Building

    mcs -target:library Hsluv/Hsluv.cs

# Testing

See `.travis.yml`.

# Packaging

    $ cd Hsluv
    $ vim Hsluv.nuspec
    $ nuget pack Hsluv.nuspec

# Authors

Mark Wonnacott, Alexei Boronine
