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

namespace Fabrica.Rules.Validators;

public interface IValidator<out TFact, out TType>
{

    IValidator<TFact, TType> Is( Func<TFact, TType, bool> condition );

    IValidator<TFact, TType> IsNot( Func<TFact, TType, bool> condition );

    IValidationRule<TFact> Otherwise( string template, params Func<TFact, object>[] parameters );

    IValidationRule<TFact> Otherwise( string group, string template, params Func<TFact, object>[] parameters );

    IValidationRule<TFact> Otherwise( EventDetail.EventCategory category, string group, string template, params Func<TFact, object>[] parameters );


}


public class Validator<TFact, TType> : BaseValidator<TFact>, IValidator<TFact,TType>
{

    public Validator( ValidationRule<TFact> rule, string group, Func<TFact, TType> extractor ) : base( rule, group )
    {
        Extractor = extractor;
    }


    protected Func<TFact, TType> Extractor { get;  }

        
    public IValidator<TFact, TType> Is( Func<TFact, TType, bool> condition )
    {
        bool Cond(TFact f) => condition(f, Extractor(f));
        Conditions.Add( Cond );
        return this;
    }

        
    public IValidator<TFact, TType> IsNot( Func<TFact, TType, bool> condition )
    {
        bool Cond(TFact f) => !(condition(f, Extractor(f)));
        Conditions.Add( Cond );
        return this;
    }

}