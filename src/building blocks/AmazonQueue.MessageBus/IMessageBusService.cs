using Amazon.SQS.Model;

namespace AmazonQueue.MessageBus
{
    public interface IMessageBusService
    {
        Task<bool> SendAsync<T>(string queueUrl, T message) where T : Event;
        Task<bool> PublishAsync<T>(string topicArn, T message) where T : Event;
        Task SubscribeAsync(Action<ReceiveMessageResponse, string> callback, string queueUrl, int maxNumberOfMessages = 10, int waitTimeSeconds = 5);
        Task DeleteAsync(string queueUrl, string receiptHandle);
        Task<string> GetQueueUrl(string queueName);
    }
}

