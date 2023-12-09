namespace AmazonQueue.MessageBus.Integration
{
    public class UserCreatedIntegrationEvent : Event
    {
        public UserCreatedIntegrationEvent(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
    }
}