using Amazon.SQS.Model;

namespace AmazonQueue.MessageBus
{
    public interface IConsumer
    {
        void RegisterConsumer(ReceiveMessageResponse message);
    }
}
