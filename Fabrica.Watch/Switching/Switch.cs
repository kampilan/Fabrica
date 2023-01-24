﻿/*
The MIT License (MIT)

Copyright (c) 2017 The Kampilan Group Inc.

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
*/

using System.Drawing;

namespace Fabrica.Watch.Switching;

public class Switch : ISwitch
{

    public static Switch Create()
    {
        var sw = new Switch();
        return sw;
    }

    public string Pattern { get; set; } = "";

    public string Tag { get; set; } = "";

    public Level Level { get; set; } = Level.Error;
    public Color Color { get; set; } = Color.White;

    public Switch WhenMatched( string pattern )
    {
        Pattern = pattern;
        return this;
    }

    public Switch UseLevel( Level level )
    {
        Level = level;
        return this;
    }

    public Switch UseColor( Color color )
    {
        Color = color;
        return this;
    }


}