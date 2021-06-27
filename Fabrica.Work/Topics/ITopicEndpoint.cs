namespace Fabrica.Work.Topics
{


    public interface ITopicEndpoint
    {

        string Topic { get; }

        string Name { get; }
        string Path { get; }

    }


}