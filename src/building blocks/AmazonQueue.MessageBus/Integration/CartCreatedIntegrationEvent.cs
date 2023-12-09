namespace AmazonQueue.MessageBus.Integration
{
    public class CartCreatedIntegrationEvent : Event
    {
        public CartCreatedIntegrationEvent(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }
    }
}