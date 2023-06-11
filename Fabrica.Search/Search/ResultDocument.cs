namespace Fabrica.Search;

public class ResultDocument
{


    public static ResultDocument Build(InputKey key, double score)
    {

        var result = new ResultDocument
        {
            Entity = key.Entity,
            Id = key.Id,
            Score = score
        };

        return result;

    }

    public string Entity { get; set; } = "";
    public long Id { get; set; }

    public double Score { get; set; }

    public void FromKey(InputKey key)
    {
        Entity = key.Entity;
        Id = key.Id;
    }


}