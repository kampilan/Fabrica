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

using Fabrica.Exceptions;
using Fabrica.Rules.Evaluation;

namespace Fabrica.Rules.Validators;

public abstract class BaseValidator<TFact>
{

        
    protected BaseValidator( ValidationRule<TFact> rule, string group )
    {

        Rule  = rule;
        Group = group;

        Conditions = new List<Func<TFact, bool>>();
        Consequence = f => { };

    }


    private string Group { get; }

    private ValidationRule<TFact> Rule { get; }


    public IList<Func<TFact, bool>> Conditions { get; }
    public Action<TFact> Consequence { get; private set; }


    public IValidationRule<TFact> Otherwise( string template, params Func<TFact, object>[] parameters )
    {
        Consequence = f => _BuildMessage( f, EventDetail.EventCategory.Violation, Group, template, parameters );
        return Rule;
    }

    public IValidationRule<TFact> Otherwise( string group, string template, params Func<TFact, object>[] parameters)
    {
        Consequence = f => _BuildMessage(f, EventDetail.EventCategory.Violation, group, template, parameters);
        return Rule;
    }

    public IValidationRule<TFact> Otherwise( EventDetail.EventCategory category, string group, string template, params Func<TFact, object>[] parameters )
    {
        Consequence = f => _BuildMessage(f, category, group, template, parameters);
        return Rule;
    }


    private void _BuildMessage( TFact fact, EventDetail.EventCategory category, string group, string template,  Func<TFact, object>[] parameters )
    {
        int len = parameters.Length;

        var markers = new object[len];
        for( int i = 0; i < len; i++ )
        {
            object o = parameters[i]( fact ) ?? "null";
            markers[i] = o;
        }

        string desc = String.Format( template, markers );
        RuleThreadLocalStorage.CurrentContext.Event( category, group, desc, fact );
    }
}