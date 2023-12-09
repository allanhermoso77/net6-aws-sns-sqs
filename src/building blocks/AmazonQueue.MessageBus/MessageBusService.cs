using Amazon;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Amazon.SQS.Util;
using System.Text.Json;

namespace AmazonQueue.MessageBus
{
    public class MessageBusService : IMessageBusService
    {
        private readonly IAmazonSQS _busQueue;
        private readonly AmazonSimpleNotificationServiceClient _busTopic;
        private ListQueuesResponse _queues;
        private ListTopicsResponse _topics;

        public MessageBusService(string accessKey, string secretKey)
        {
            var credentials = new BasicAWSCredentials(accessKey, secretKey);

            _busQueue = CreateBusQueue(credentials);
            _busTopic = CreateBusTopic(credentials);

            Scaffold();
        }

        public async Task<bool> SendAsync<T>(string queueUrl, T message) where T : Event
        {
            try
            {
                var contentJson = JsonSerializer.Serialize<object>(message);
                var sendRequest = new SendMessageRequest(queueUrl, contentJson);

                var response = await _busQueue.SendMessageAsync(sendRequest);

                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> PublishAsync<T>(string topicArn, T message) where T : Event
        {
            try
            {
                var contentJson = JsonSerializer.Serialize<object>(message);

                var sendRequest = new PublishRequest()
                {
                    TopicArn = topicArn,
                    Message = contentJson
                };

                var response = await _busTopic.PublishAsync(sendRequest);

                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SubscribeAsync(Action<ReceiveMessageResponse, string> callback, string queueUrl, int maxNumberOfMessages = 10, int waitTimeSeconds = 5)
        {
            try
            {
                var request = new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl,
                    MaxNumberOfMessages = maxNumberOfMessages,
                    WaitTimeSeconds = waitTimeSeconds
                };

                var response = await _busQueue.ReceiveMessageAsync(request);

                callback(response, queueUrl);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task DeleteAsync(string queueUrl, string receiptHandle)
        {
            try
            {
                var request = new DeleteMessageRequest
                {
                    QueueUrl = queueUrl,
                    ReceiptHandle = receiptHandle
                };

                await _busQueue.DeleteMessageAsync(request);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> GetQueueUrl(string queueName)
        {
            try
            {
                var request = new GetQueueUrlRequest
                {
                    QueueName = queueName
                };

                var response = await _busQueue.GetQueueUrlAsync(request);

                return response.QueueUrl;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> GetTopicArn(string topicName)
        {
            try
            {
                var matches = _topics.Topics.Where(x => x.TopicArn.EndsWith(topicName)).FirstOrDefault();

                if (matches is null) return "";

                return matches.TopicArn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private IAmazonSQS CreateBusQueue(BasicAWSCredentials credentials)
        {
            var region = RegionEndpoint.USEast1;

            return new AmazonSQSClient(credentials, region);
        }

        private AmazonSimpleNotificationServiceClient CreateBusTopic(BasicAWSCredentials credentials)
        {
            var region = RegionEndpoint.USEast1;

            return new AmazonSimpleNotificationServiceClient(credentials, region);
        }

        private async Task Scaffold()
        {
            try
            {
                _queues = await _busQueue.ListQueuesAsync("");
                _topics = await _busTopic.ListTopicsAsync();

                foreach (var item in StructureStack.List)
                {
                    // Se for tópico
                    if (item.Bind.Length > 0)
                    {
                        await CreateTopic(item);
                    }
                    else
                    {
                        await CreateQueue(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task CreateQueue(Structure item)
        {
            try
            {
                if (isQueueExistAsync(item.Name)) return;

                var attributes = new Dictionary<string, string>();
                attributes.Add(QueueAttributeName.VisibilityTimeout, item.VisibilityTimeout);

                var queue = new CreateQueueRequest
                {
                    QueueName = item.Name,
                    Attributes = attributes
                };

                var response = await _busQueue.CreateQueueAsync(queue);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task CreateTopic(Structure item)
        {
            try
            {
                if (isTopicExistAsync(item.Name)) return;

                var topic = new CreateTopicRequest
                {
                    Name = item.Name
                };

                var responseCreate = await _busTopic.CreateTopicAsync(topic);

                if (responseCreate.HttpStatusCode != System.Net.HttpStatusCode.OK) return;

                foreach (var subscription in item.Bind)
                {
                    if (await isSubscriptionExists(responseCreate.TopicArn, subscription)) return;

                    var responseQueue = await _busQueue.GetQueueUrlAsync(subscription);

                    var responseAttributes = await _busQueue.GetAttributesAsync(responseQueue.QueueUrl);

                    var queueArn = responseAttributes[SQSConstants.ATTRIBUTE_QUEUE_ARN];

                    var responseSubscribe = await _busTopic.SubscribeAsync(new SubscribeRequest
                    {
                        TopicArn = responseCreate.TopicArn,
                        Protocol = "sqs",
                        Endpoint = queueArn
                    });

                    await ChangeSQSPolicyAsync(responseCreate.TopicArn, queueArn, responseQueue.QueueUrl);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private bool isTopicExistAsync(string name)
        {
            var matches = _topics.Topics.Where(x => x.TopicArn.EndsWith(name));

            if (!matches.Any()) return false;

            return true;
        }

        private bool isQueueExistAsync(string name)
        {
            var matches = _queues.QueueUrls.Where(x => x.EndsWith(name));

            if (!matches.Any()) return false;

            return true;
        }

        private async Task<bool> isSubscriptionExists(string name, string subscription)
        {
            var matcheSubscription = _queues.QueueUrls.Where(x => x.EndsWith(subscription)).FirstOrDefault();

            if (matcheSubscription is null) return false;

            var request = new ListSubscriptionsByTopicRequest
            {
                TopicArn = name
            };

            var response = await _busTopic.ListSubscriptionsByTopicAsync(request);

            var matches = response.Subscriptions.Where(x => x.Endpoint.EndsWith(subscription));

            if (!matches.Any()) return false;

            return true;
        }

        private async Task ChangeSQSPolicyAsync(string topicArn, string queueArn, string queueUrl)
        {
            try
            {
                var policy = PolicySQS.AllowSend.Replace("%QueueArn%", queueArn).Replace("%TopicArn%", topicArn);

                var attributes = new Dictionary<string, string>();
                attributes.Add("Policy", policy);

                var request = new SetQueueAttributesRequest()
                {
                    Attributes = attributes,
                    QueueUrl = queueUrl
                };

                var response = await _busQueue.SetQueueAttributesAsync(request);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public class MessageSQS
    {
        public string Message { get; set; }
    }
}
