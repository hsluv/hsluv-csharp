[![Build Status](https://travis-ci.org/hsluv/husl.cs.svg?branch=master)](https://travis-ci.org/hsluv/husl.cs)
[![Package Version](https://img.shields.io/nuget/v/Hsluv.svg)](https://www.nuget.org/packages/Hsluv)

[Explanation, demo, ports etc.](http://www.hsluv.org)

# API

This library provides the `Hsluv` namespace with `ColorConverter` class with
the following static methods. Tuples are three items each: R, G, B and H, S, L.

    IList<double> HsluvToRgb(IList<double> tuple)
	IList<double> RgbToHsluv(IList<double> tuple)
	IList<double> HpluvToRgb(IList<double> tuple)
	IList<double> RgbToHpluv(IList<double> tuple)

	string HsluvToHex(IList<double> tuple)
	string HpluvToHex(IList<double> tuple)
	IList<double> HexToHsluv(string s)
	IList<double> HexToHpluv(string s)

# Building and Testing

See `Dockerfile` for instructions or run `docker build .`

# Packaging

    $ cd Hsluv
    $ vim Hsluv.nuspec
    $ nuget pack Hsluv.nuspec

# Authors

Mark Wonnacott, Alexei Boronine

# License

Copyright (c) 2015 Alexei Boronine

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
