namespace AmazonQueue.MessageBus
{
    public static class PolicySQS
    {
        public const string AllowSend =
                                                @"{
                                                      ""Statement"": [
                                                        {                                                          
                                                          ""Effect"": ""Allow"",
                                                          ""Principal"": {
                                                            ""AWS"": ""*""
                                                          },
                                                          ""Action"": ""sqs:SendMessage"",
                                                          ""Resource"": ""%QueueArn%"",
                                                          ""Condition"": {
                                                            ""ArnEquals"": {
                                                              ""aws:SourceArn"": ""%TopicArn%""
                                                            }
                                                          }
                                                        }
                                                      ]
                                                    }";
    }

    public static class QueueTypes
    {
        public const string AWS_SQS_USER_CREATED_AUTH = "AWS_SQS_USER_CREATED_AUTH_XXX";
        public const string AWS_SQS_USER_CREATED_CUSTOMER = "AWS_SQS_USER_CREATED_CUSTOMER_XXX";
        public const string AWS_SQS_CART_CREATED = "AWS_SQS_CART_CREATED_XXX";
    }

    public static class TopicTypes
    {
        public const string AWS_SNS_USER_CREATED = "AWS_SNS_USER_CREATED_XXX";
    }

    public static class StructureStack
    {
        public static List<Structure> List = new List<Structure>()
        {
            // Queues
            new Structure(QueueTypes.AWS_SQS_CART_CREATED, "120", new string[] {}),
            new Structure(QueueTypes.AWS_SQS_USER_CREATED_AUTH, "120", new string[] {}),
            new Structure(QueueTypes.AWS_SQS_USER_CREATED_CUSTOMER, "120", new string[] {}),
            // Topics
            new Structure(TopicTypes.AWS_SNS_USER_CREATED, null, new string[] {QueueTypes.AWS_SQS_USER_CREATED_AUTH, QueueTypes.AWS_SQS_USER_CREATED_CUSTOMER}),
        };
    }

    public class Structure
    {
        public Structure(string name, string visibilityTimeout, string[] bind = null)
        {
            Name = name;
            VisibilityTimeout = visibilityTimeout;
            Bind = bind;
        }
        public string Name { get; set; }
        public string VisibilityTimeout { get; set; }
        public string[] Bind { get; set; }
    }
}
