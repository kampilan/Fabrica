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


using System;
using Fabrica.Watch;
using GemBox.Document;

namespace Fabrica.Press.Generation.Transformers
{

    public class FuncTransformer<TInline>: IInlineTransformer where TInline: Inline
    {

        public FuncTransformer( Func<TInline,Inline> theAction )
        {
            TheAction = theAction;
        }

        public Func<TInline,Inline> TheAction { get; }

        public Inline Handle( Inline target )
        {

            var result = target;
            if( target is TInline inline )
                result = TheAction( inline );
            else
                this.GetLogger().DebugFormat( "Inline type mismatch, Expecting: ({0}) Received: ({1})", typeof(TInline).FullName, target.GetType().FullName );

            return result;

        }


    }


}
