using Fabrica.Utilities.Container;
using System.Collections.Generic;
using System.Linq;
using Humanizer;

namespace Fabrica.Work.Processor.Parsers;

public class WorkTopicTransformer: CorrelatedObject
{

    public WorkTopicTransformer(ICorrelation correlation) : base(correlation)
    {
    }

    public int PrefixCount { get; set; } = 0;
    public int SuffixCount { get; set; } = 0;

    public string Prepend { get; set; } = "";
    public string Separator { get; set; } = "";
    public string DefaultName { get; set; } = "root";


    public string Transform( string source )
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to split source by '/'");
        var segs = source.Split("/");

        logger.Inspect(nameof(segs.Length), segs.Length);



        // *****************************************************************
        logger.Debug("Attempting to transform source to Topic Name");
        var list = new List<string>();
        if( !string.IsNullOrWhiteSpace(Prepend) )
            list.Add(Prepend.ToLowerInvariant());

        list.AddRange(segs.Skip(PrefixCount).SkipLast(SuffixCount));

        if( (!string.IsNullOrWhiteSpace(Prepend) && list.Count == 1) || list.Count == 0 ) 
            list.Add(DefaultName.ToLowerInvariant());

        var topic = string.Join(Separator, list.Select(s=>s.Humanize() ).Select(s => s.Titleize() ).Select(s => s.Replace(" ", "")));

        logger.Inspect(nameof(topic), topic);



        // *****************************************************************
        return topic;

    }


}