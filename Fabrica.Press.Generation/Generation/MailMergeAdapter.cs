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


using System.Collections.Generic;
using Fabrica.Press.Generation.DataSources;
using Fabrica.Press.Generation.Formatters;
using Fabrica.Press.Generation.Transformers;
using Fabrica.Watch;
using GemBox.Document.MailMerging;

namespace Fabrica.Press.Generation
{
    

    public class MailMergeAdapter: IMailMergeDataSource
    {


        public MailMergeAdapter( IReadOnlyDictionary<string, IMergeFieldFormatter> formatters )
        {

            Formatters = formatters;

            Logger = GetLogger();

        }

        private IReadOnlyDictionary<string,IMergeFieldFormatter> Formatters { get; }


        public ILogger Logger { get; set; }
        protected ILogger GetLogger()
        {
            return Logger;
        }


        private IMergeDataSource _source;
        public IMergeDataSource Source
        {
            get => _source;
            set
            {
                _source = value;
                _global = true;
                _source.Rewind();
            }
        }

        private bool _global;
        public IMergeDataSource Global
        {
            get => _source;
            set
            {
                _source = value;
                _global = true;
                _source.Rewind();
            }
        }


        public void WhenFieldMerging( FieldMergingEventArgs args )
        {

            if( args.IsValueFound && args.Value is IInlineTransformer handler )
            {
                Logger.DebugFormat("IInlineTransformer field ({0}) encountered", args.FieldName);
                args.Inline = handler.Handle(args.Inline);
            }

            if ( !args.IsValueFound )
                Logger.DebugFormat( "Could not find Field ({1} in Region ({0}))", args.MergeContext.RangeName, args.FieldName );

        }


        public string RegionEnterPrefix { get; set; } = "Enter:";
        public string RegionExitPrefix { get; set; } = "Exit:";



        #region IMailMergeDataSource implementation


        string IMailMergeDataSource.Name => _global?"":Source.Region;

        bool IMailMergeDataSource.MoveNext()
        {
            return Source.MoveNext();
        }


        bool IMailMergeDataSource.TryGetValue( string valueName, out object value )
        {

            var mf = MergeField.Parse(valueName);

            var found = Source.TryGetValue( mf.Name, out var raw );
            if( found && Formatters.ContainsKey(mf.Tag) )
                value = Formatters[mf.Tag].Render(raw);
            else if (found)
                value = raw.ToString();
            else
                value = "";

            return found;

        }

        #endregion



    }




}
