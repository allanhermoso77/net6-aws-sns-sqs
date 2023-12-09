namespace AmazonQueue.MessageBus
{
    public class Event
    {
        public Guid IdEvent { get; private set; }
        public DateTime Timestamp { get; private set; }

        protected Event()
        {
            Timestamp = DateTime.Now;
            IdEvent = new Guid();
        }
    }
}

