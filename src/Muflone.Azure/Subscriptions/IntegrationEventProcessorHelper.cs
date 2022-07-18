using Microsoft.Extensions.DependencyInjection;
using Muflone.Azure.Factories;
using Muflone.Factories;
using Muflone.Messages.Events;

namespace Muflone.Azure.Subscriptions;

public static class IntegrationEventProcessorHelper
{
    public static IServiceCollection AddIntegrationEventProcessor<T>(this IServiceCollection services,
        string connectionString, string subscriptionName) where T : IntegrationEvent
    {
        services.AddScoped(provider =>
        {
            var integrationEventHandlerFactory = provider.GetService<IIntegrationEventHandlerFactoryAsync>();
            var messageMapperFactory = provider.GetService<IMessageMapperFactory>();

            var brokerOptions = new BrokerOptions
            {
                ConnectionString = connectionString,
                TopicName = nameof(T).ToLower(),
                SubscriptionName = subscriptionName
            };

            return messageMapperFactory is null
                ? RegisterIntegrationEventProcessorWithoutMessageMapper<T>(brokerOptions, integrationEventHandlerFactory)
                : RegisterIntegrationEventProcessorWithMessageMapper<T>(brokerOptions, integrationEventHandlerFactory,
                    messageMapperFactory);
        });

        return services;
    }

    private static IIntegrationEventProcessorAsync<T> RegisterIntegrationEventProcessorWithMessageMapper<T>(
        BrokerOptions brokerOptions, IIntegrationEventHandlerFactoryAsync integrationEventHandlerFactory, IMessageMapperFactory messageMapperFactory) where T : IntegrationEvent
    {
        var domainEventConsumerFactory =
            new ServiceBusIntegrationEventProcessorFactory<T>(brokerOptions, messageMapperFactory,
                integrationEventHandlerFactory);
        return domainEventConsumerFactory.IntegrationEventProcessorAsync;
    }

    private static IIntegrationEventProcessorAsync<T> RegisterIntegrationEventProcessorWithoutMessageMapper<T>(
        BrokerOptions brokerOptions, IIntegrationEventHandlerFactoryAsync integrationEventHandlerFactory) where T : IntegrationEvent
    {
        var domainEventConsumerFactory =
            new ServiceBusIntegrationEventProcessorFactory<T>(brokerOptions, integrationEventHandlerFactory);
        return domainEventConsumerFactory.IntegrationEventProcessorAsync;
    }
}