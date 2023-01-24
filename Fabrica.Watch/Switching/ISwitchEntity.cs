namespace Fabrica.Watch.Switching;

public interface ISwitchEntity
{

    string Uid { get; set; }

    string Pattern { get; set; }


    string FilterType { get; set; }

    string FilterTarget { get; set; }


    string Tag { get; set; }

    string Level { get; set; }

    string Color { get; set; }


}