using System;
using System.Linq;
using Muflone.Azure.ServiceBus;
using Muflone.Factories;
using Muflone.Messages.Events;

namespace Muflone.Azure.Factories;

public class ServiceBusIntegrationEventProcessorFactory<T> where T : class, IIntegrationEvent
{
    public readonly IIntegrationEventProcessorAsync<T> IntegrationEventProcessorAsync;

    public ServiceBusIntegrationEventProcessorFactory(BrokerOptions brokerOptions,
        IMessageMapperFactory messageMapperFactory)
    {
        var messageMapper = messageMapperFactory.CreateMessageMapper<T>();

        if (messageMapper == null)
            throw new Exception(
                $"No MessageMapper has found for {(object)typeof(T)}. A MessageMapper must be specified for every IntegrationEvent");

        IntegrationEventProcessorAsync = new AzureIntegrationEventProcessorAsync<T>(brokerOptions, messageMapper);
    }

    public ServiceBusIntegrationEventProcessorFactory(BrokerOptions brokerOptions)
    {
        IntegrationEventProcessorAsync = new AzureIntegrationEventProcessorAsync<T>(brokerOptions);
    }

    public ServiceBusIntegrationEventProcessorFactory(BrokerOptions brokerOptions,
        IMessageMapperFactory messageMapperFactory,
        IIntegrationEventHandlerFactoryAsync eventHandlerFactoryAsync)
    {
        var messageMapper = messageMapperFactory.CreateMessageMapper<T>();
        var integrationEventHandlerAsync = eventHandlerFactoryAsync.CreateIntegrationEventHandlerAsync<T>();
        var integrationEventHandlersAsync = eventHandlerFactoryAsync.CreateIntegrationEventHandlersAsync<T>();

        if (messageMapper == null)
            throw new Exception(
                $"No MessageMapper has found for {(object)typeof(T)}. A MessageMapper must be specified for every DomainEvent");

        if (integrationEventHandlerAsync == null)
            throw new Exception(
                $"No EventHandler has found for {(object)typeof(T)}. At least an EventHandler must be specified for this DomainEvent");

        var integrationEventHandlersArray = integrationEventHandlersAsync as IIntegrationEventHandlerAsync<T>[] ?? integrationEventHandlersAsync.ToArray();
        if (!integrationEventHandlersArray.Any())
            throw new Exception(
                $"No EventHandler has found for {(object)typeof(T)}. At least an EventHandler must be specified for this DomainEvent");

        IntegrationEventProcessorAsync = new AzureIntegrationEventProcessorAsync<T>(brokerOptions, messageMapper, integrationEventHandlersArray);
    }

    public ServiceBusIntegrationEventProcessorFactory(BrokerOptions brokerOptions,
        IIntegrationEventHandlerFactoryAsync eventHandlerFactoryAsync)
    {
        var integrationEventHandlerAsync = eventHandlerFactoryAsync.CreateIntegrationEventHandlerAsync<T>();
        var integrationEventHandlersAsync = eventHandlerFactoryAsync.CreateIntegrationEventHandlersAsync<T>();

        if (integrationEventHandlerAsync == null)
            throw new Exception(
                $"No EventHandler has found for {(object)typeof(T)}. At least an EventHandler must be specified for this DomainEvent");

        var integrationEventHandlersArray = integrationEventHandlersAsync as IIntegrationEventHandlerAsync<T>[] ?? integrationEventHandlersAsync.ToArray();
        if (!integrationEventHandlersArray.Any())
            throw new Exception(
                $"No EventHandler has found for {(object)typeof(T)}. At least an EventHandler must be specified for this DomainEvent");

        IntegrationEventProcessorAsync = new AzureIntegrationEventProcessorAsync<T>(brokerOptions, integrationEventHandlersArray);
    }
}