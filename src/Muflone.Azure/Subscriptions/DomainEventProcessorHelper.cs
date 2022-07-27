using Microsoft.Extensions.DependencyInjection;
using Muflone.Azure.Factories;
using Muflone.Factories;
using Muflone.Messages.Events;

namespace Muflone.Azure.Subscriptions;

public static class DomainEventProcessorHelper
{
    public static IServiceCollection AddDomainEventProcessor<T>(this IServiceCollection services,
        string connectionString, string subscriptionName) where T : DomainEvent
    {
        services.AddScoped(provider =>
        {
            var domainEventHandlerFactory = provider.GetService<IDomainEventHandlerFactoryAsync>();
            var messageMapperFactory = provider.GetService<IMessageMapperFactory>();

            var brokerOptions = new BrokerOptions
            {
                ConnectionString = connectionString,
                TopicName = typeof(T).Name.ToLower(),
                SubscriptionName = subscriptionName
            };

            return messageMapperFactory is null
                ? RegisterDomainEventProcessorWithoutMessageMapper<T>(brokerOptions, domainEventHandlerFactory)
                : RegisterDomainEventProcessorWithMessageMapper<T>(brokerOptions, domainEventHandlerFactory,
                    messageMapperFactory);
        });

        return services;
    }

    private static IDomainEventProcessorAsync<T> RegisterDomainEventProcessorWithMessageMapper<T>(
        BrokerOptions brokerOptions, IDomainEventHandlerFactoryAsync domainEventHandlerFactory, IMessageMapperFactory messageMapperFactory) where T : DomainEvent
    {
        var domainEventConsumerFactory =
            new ServiceBusEventProcessorFactory<T>(brokerOptions, messageMapperFactory,
                domainEventHandlerFactory);
        return domainEventConsumerFactory.DomainEventProcessorAsync;
    }

    private static IDomainEventProcessorAsync<T> RegisterDomainEventProcessorWithoutMessageMapper<T>(
        BrokerOptions brokerOptions, IDomainEventHandlerFactoryAsync domainEventHandlerFactory) where T : DomainEvent
    {
        var domainEventConsumerFactory =
            new ServiceBusEventProcessorFactory<T>(brokerOptions, domainEventHandlerFactory);
        return domainEventConsumerFactory.DomainEventProcessorAsync;
    }
}