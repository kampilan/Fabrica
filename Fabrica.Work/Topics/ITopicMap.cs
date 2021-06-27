namespace Fabrica.Work.Topics
{

    
    public interface ITopicMap
    {

        bool HasTopic( string topic );
        bool TryGetEndpoint( string topic, out ITopicEndpoint endpoint );

    }


}