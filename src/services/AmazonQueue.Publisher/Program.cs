using AmazonQueue.MessageBus;
using AmazonQueue.MessageBus.Integration;

namespace AmazonQueue.Publisher
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var busService = new MessageBusService("__AWS_ACCESS_KEY__", "__AWS_SECRET_KEY__");
            int count = 1;
            int numberOfMessages = 3;

            while (count <= numberOfMessages)
            {
                Console.Write("Cart Code: ");

                Random rnd = new Random();
                int id = rnd.Next();

                // Queue
                var evt = new CartCreatedIntegrationEvent(id, "Anderson");
                var queueUrl = await busService.GetQueueUrl(QueueTypes.AWS_SQS_CART_CREATED);
                await busService.SendAsync(queueUrl, evt);

                // Topic
                // var evt = new UserCreatedIntegrationEvent(id);
                // var topicArn = await busService.GetTopicArn(TopicTypes.AWS_SNS_USER_CREATED);
                // await busService.PublishAsync(topicArn, evt);

                count++;
            }
        }
    }
}
