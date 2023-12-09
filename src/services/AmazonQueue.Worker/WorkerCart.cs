using Amazon.SQS.Model;
using AmazonQueue.MessageBus;
using AmazonQueue.MessageBus.Integration;
using System.Text.Json;

namespace AmazonQueue.Worker
{
    public class WorkerCart : BackgroundService
    {
        private readonly IMessageBusService _busService;

        public WorkerCart(IMessageBusService busService)
        {
            _busService = busService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var queueUrl = await _busService.GetQueueUrl(QueueTypes.AWS_SQS_CART_CREATED);

                await Start(queueUrl, stoppingToken);
            }
            catch (QueueDoesNotExistException)
            {
                Console.WriteLine("Queue not exist error");
            }
            catch (Exception e)
            {
                Console.WriteLine("Format error! " + e.Message);
            }
        }

        private async Task Start(string queueUrl, CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _busService.SubscribeAsync(RegisterConsumer, queueUrl);
            }
        }

        private void RegisterConsumer(ReceiveMessageResponse response, string queueUrl)
        {
            var messages = response.Messages.Any() ? response.Messages : new List<Message> { };

            if (messages.Any())
            {
                foreach (var msg in messages)
                {
                    var isMessageProcessed = ProcessMessage(msg);

                    if (isMessageProcessed)
                    {
                        var task = Task.Run(async () =>
                        {
                            await _busService.DeleteAsync(queueUrl, msg.ReceiptHandle);
                        });
                        task.Wait();
                    }
                }
            }
            else
            {
                Task.Delay(TimeSpan.FromSeconds(5)).Wait();
            }
        }
        private bool ProcessMessage(Message msg)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            var cart = JsonSerializer.Deserialize<CartCreatedIntegrationEvent>(msg.Body, options);

            Console.WriteLine($"Cart Received : {cart.Id}");

            return true;
        }
    }
}