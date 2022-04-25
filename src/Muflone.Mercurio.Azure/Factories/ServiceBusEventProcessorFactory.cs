using System;
using System.Linq;
using Muflone.Factories;
using Muflone.Mercurio.Azure.ServiceBus;
using Muflone.Messages.Events;

namespace Muflone.Mercurio.Azure.Factories;

public class ServiceBusEventProcessorFactory<T> where T : class, IDomainEvent
{
    public readonly IDomainEventProcessorAsync<T> DomainEventProcessorAsync;

    public ServiceBusEventProcessorFactory(BrokerOptions brokerOptions,
        IMessageMapperFactory messageMapperFactory)
    {
        var messageMapper = messageMapperFactory.CreateMessageMapper<T>();

        if (messageMapper == null)
            throw new Exception(
                $"No MessageMapper has found for {(object)typeof(T)}. A MessageMapper must be specified for every DomainEvent");

        DomainEventProcessorAsync = new AzureDomainEventProcessorAsync<T>(brokerOptions, messageMapper);
    }

    public ServiceBusEventProcessorFactory(BrokerOptions brokerOptions)
    {
        DomainEventProcessorAsync = new AzureDomainEventProcessorAsync<T>(brokerOptions);
    }

    public ServiceBusEventProcessorFactory(BrokerOptions brokerOptions,
        IMessageMapperFactory messageMapperFactory,
        IDomainEventHandlerFactoryAsync eventHandlerFactoryAsync)
    {
        var messageMapper = messageMapperFactory.CreateMessageMapper<T>();
        var domainEventHandlersAsync = eventHandlerFactoryAsync.CreateDomainEventHandlersAsync<T>();

        if (messageMapper == null)
            throw new Exception(
                $"No MessageMapper has found for {(object)typeof(T)}. A MessageMapper must be specified for every DomainEvent");

        var domainEventHandlersArray = domainEventHandlersAsync as IDomainEventHandlerAsync<T>[] ?? domainEventHandlersAsync.ToArray();
        if (!domainEventHandlersArray.Any())
            throw new Exception(
                $"No EventHandler has found for {(object)typeof(T)}. At least an EventHandler must be specified for this DomainEvent");

        DomainEventProcessorAsync = new AzureDomainEventProcessorAsync<T>(brokerOptions, messageMapper, domainEventHandlersArray);
    }

    public ServiceBusEventProcessorFactory(BrokerOptions brokerOptions,
        IDomainEventHandlerFactoryAsync eventHandlerFactoryAsync)
    {
        var domainEventHandlersAsync = eventHandlerFactoryAsync.CreateDomainEventHandlersAsync<T>();

        var domainEventHandlersArray = domainEventHandlersAsync as IDomainEventHandlerAsync<T>[] ?? domainEventHandlersAsync.ToArray();
        if (!domainEventHandlersArray.Any())
            throw new Exception(
                $"No EventHandler has found for {(object)typeof(T)}. At least an EventHandler must be specified for this DomainEvent");

        DomainEventProcessorAsync = new AzureDomainEventProcessorAsync<T>(brokerOptions, domainEventHandlersArray);
    }
}