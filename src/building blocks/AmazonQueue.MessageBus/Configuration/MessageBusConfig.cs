using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AmazonQueue.MessageBus.Configuration
{
    public static class MessageBusConfig
    {
        public static IServiceCollection AddMessageBus(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IMessageBusService>(new MessageBusService(configuration["AWSCredentials:AccessKey"], configuration["AWSCredentials:SecretKey"]));

            return services;
        }
    }
}