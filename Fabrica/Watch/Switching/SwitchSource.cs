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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;

namespace Fabrica.Watch.Switching
{


    public class SwitchSource : ISwitchSource
    {


        [NotNull]
        public SwitchSource WhenNotMatched( Level level )
        {
            var sw = new Switch {Level = level, Color = Color.LightGray};
            DefaultSwitch = sw;
            return this;
        }

        [NotNull]
        public SwitchSource WhenNotMatched( Level level, Color color )
        {
            var sw = new Switch { Level = level, Color = color };
            DefaultSwitch = sw;
            return this;
        }

        [NotNull]
        public SwitchSource WhenMatched( string pattern, string tag, Level level, Color color )
        {

            var switches = Switches.Select(p => new SwitchDef
            {

                Pattern = p.Value.Pattern,
                Tag     = p.Value.Tag,
                Level   = p.Value.Level,
                Color   = p.Value.Color

            }).ToList();

            var sw = new SwitchDef
            {
                Pattern = pattern,
                Tag     = tag,
                Level   = level,
                Color   = color
            };

            switches.Add(sw);


            Update( switches );


            return this;

        }


        public ISwitch DefaultSwitch { get; set; } = new Switch { Level = Level.Error, Color = Color.LightGray };
        public ISwitch DebugSwitch { get; set; } = new Switch { Level = Level.Debug, Color = Color.HotPink };


        private readonly ReaderWriterLockSlim _switchLock = new ReaderWriterLockSlim();

        protected IReadOnlyCollection<string> Patterns { get; set; } = new ReadOnlyCollection<string>(new List<string>());
        protected IReadOnlyCollection<string> Filters { get; set; } = new ReadOnlyCollection<string>(new List<string>());

        protected IDictionary<string, ISwitch> Switches { get; set; } = new ConcurrentDictionary<string, ISwitch>();



        public virtual void Start()
        {
        }

        public virtual void Stop()
        {

        }


        public virtual ISwitch Lookup( string category )
        {

            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(category));

            try
            {


                _switchLock.EnterReadLock();



                // ************************************************************************
                if( Patterns.Count == 0 )
                    return DefaultSwitch;



                // ************************************************************************
                var lu2      = Patterns.FirstOrDefault(category.StartsWith)??"";
                var lu2Found = Switches.TryGetValue( lu2, out var psw );

                if (lu2Found)
                    return psw;



                // ************************************************************************
                return DefaultSwitch;


            }
            finally
            {
                _switchLock.ExitReadLock();
            }

        }



        public virtual bool Lookup( string filterType, string filterTarget, string category, out ISwitch found )
        {


            try
            {


                var composite =  _buildComposite( filterType, filterTarget, category );


                _switchLock.EnterReadLock();



                // ************************************************************************
                if( Filters.Count == 0 )
                {
                    found = DefaultSwitch;
                    return false;
                }



                // ************************************************************************
                var lu1      = Filters.FirstOrDefault(composite.StartsWith)??"";
                var lu1Found = Switches.TryGetValue( lu1, out var psw );

                if( lu1Found )
                {
                    found = psw;
                    return true;
                }



                // ************************************************************************
                found = DefaultSwitch;
                return false;


            }
            finally
            {
                _switchLock.ExitReadLock();
            }


        }


        public ISwitch GetDefaultSwitch()
        {
            return DefaultSwitch;
        }

        public ISwitch GetDebugSwitch()
        {
            return DebugSwitch;
        }


        public IList<SwitchDef> CurrentSwitchDefs
        {
            get
            {
                return Switches.Values.Select( s =>new SwitchDef {Pattern = s.Pattern, Color = s.Color, Level = s.Level, Tag = s.Tag} ).ToList();
            }
        }


        public virtual void Update( [NotNull] IEnumerable<SwitchDef> switchSource )
        {


            if( switchSource == null) throw new ArgumentNullException(nameof(switchSource));


            var switches = new ConcurrentDictionary<string, ISwitch>();


            
            // ***************************************************************
            var pKeys = new List<string>();
            var fKeys = new List<string>();
            foreach( var def in switchSource.Where(s=>s.Level != Level.Quiet) )
            {

                var sw = new Switch
                {
                    Pattern     = def.Pattern,
                    Tag         = def.Tag,
                    Color       = def.Color,
                    Level       = def.Level,
                };


                var key = def.Pattern;
                if( string.IsNullOrWhiteSpace(def.FilterType) )
                    pKeys.Add(key);
                else
                {
                    key = _buildComposite( def.FilterType, def.FilterTarget, def.Pattern);
                    fKeys.Add( key );
                }


                switches[key] = sw;


            }            


            var pOrdered = pKeys.OrderBy(k => k.Length).Reverse().ToList();
            var patterns = new ReadOnlyCollection<string>(pOrdered);

            var fOrdered = fKeys.OrderBy(k => k.Length).Reverse().ToList();
            var filters  = new ReadOnlyCollection<string>(fOrdered);



            try
            {

                _switchLock.EnterWriteLock();

                Patterns = patterns;
                Filters  = filters;

                Switches = switches;

            }
            finally
            {
                _switchLock.ExitWriteLock();
            }


        }


        [NotNull]
        private string _buildComposite(string filterType, string filterTarget, string pattern)
        {
            var composite = $"{filterType}:{filterTarget}{pattern}";
            return composite;
        }


    }


}
