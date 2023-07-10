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

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Fabrica.Watch.Switching;

public class SwitchSource : ISwitchSource
{


    public SwitchSource WhenNotMatched( Level level )
    {
        var sw = new Switch {Level = level, Color = Color.LightGray};
        DefaultSwitch = sw;
        return this;
    }

    public SwitchSource WhenNotMatched( Level level, Color color )
    {
        var sw = new Switch { Level = level, Color = color };
        DefaultSwitch = sw;
        return this;
    }

    public SwitchSource WhenMatched( string pattern, string tag, Level level, Color color )
    {

        if (level == Level.Quiet)
            return this;


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
    public ISwitch DebugSwitch { get; set; } = new Switch { Level = Level.Debug, Color = Color.PapayaWhip };


    private readonly ReaderWriterLockSlim _switchLock = new ();

    protected IReadOnlyCollection<string> Patterns { get; set; } = new ReadOnlyCollection<string>(new List<string>());

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
//            var match = Patterns.FirstOrDefault(category.StartsWith);
//            if( match is null )
//                return DefaultSwitch;


            string? match = null;
            var pc = Patterns.Count;
            for( var i = 0; i < pc; i++ )
            {

                if (!category.StartsWith(Patterns.ElementAt(i))) 
                    continue;

                match = Patterns.ElementAt(i);
                break;

            }

            if (match is null)
                return DefaultSwitch;



            // ************************************************************************
            var lu2Found = Switches.TryGetValue( match, out var psw );
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


    public virtual void Update( IEnumerable<SwitchDef> switchSource )
    {


        if( switchSource == null) throw new ArgumentNullException(nameof(switchSource));


        var switches = new ConcurrentDictionary<string, ISwitch>();

            
        // ***************************************************************
        var pKeys = new List<string>();
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
                pKeys.Add(key);

            switches[key] = sw;


        }            


        var pOrdered = pKeys.OrderBy(k => k.Length).Reverse().ToList();
        var patterns = new ReadOnlyCollection<string>(pOrdered);


        try
        {

            _switchLock.EnterWriteLock();

            Patterns = patterns;
            Switches = switches;

        }
        finally
        {
            _switchLock.ExitWriteLock();
        }


    }


    private string _buildComposite(string filterType, string filterTarget, string pattern)
    {
        var composite = $"{filterType}:{filterTarget}{pattern}";
        return composite;
    }


}