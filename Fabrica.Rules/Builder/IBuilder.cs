/*
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

using Fabrica.Rules.Tree;

namespace Fabrica.Rules.Builder;

/// <summary>
///  Types that implement this interface create rules to added to a RuleSet for
/// runtime evaluation
/// </summary>
public interface IBuilder
{

    /// <summary>
    /// Gets the set name these rules are associated with. Typically, this is the
    /// namespace the rules are defined in but this can be changed to any name desired
    /// </summary>
    string SetName { get; }


    /// <summary>
    /// Gets the rules that this builder defines. This is nomally called to add the
    /// rules to a rule set for eventual evealuation
    /// </summary>
    void LoadRules( IRuleSink sink );


}