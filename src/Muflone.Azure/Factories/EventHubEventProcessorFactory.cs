using System;
using Muflone.Azure.EventHub;
using Muflone.Factories;
using Muflone.Messages.Events;

namespace Muflone.Azure.Factories;

public class EventHubEventProcessorFactory<T> where T : class, IDomainEvent
{
	public readonly IDomainEventProcessorAsync<T> DomainEventProcessorAsync;

	public EventHubEventProcessorFactory(BrokerOptions brokerOptions,
		IMessageMapperFactory messageMapperFactory)
	{
		var messageMapper = messageMapperFactory.CreateMessageMapper<T>();

		DomainEventProcessorAsync = messageMapper != null
			? new AzureDomainEventProcessorAsync<T>(brokerOptions, messageMapper)
			: new AzureDomainEventProcessorAsync<T>(brokerOptions);
	}

	public EventHubEventProcessorFactory(BrokerOptions brokerOptions,
		IMessageMapperFactory messageMapperFactory,
		IIoTMapper ioTMapper)
	{
		var messageMapper = messageMapperFactory.CreateMessageMapper<T>();

		if (messageMapper == null)
			throw new Exception(
				$"No MessageMapper has found for {typeof(T)}. A MessageMapper must be specified for every DomainEvent");

		DomainEventProcessorAsync =
			new AzureDomainEventProcessorAsync<T>(brokerOptions, messageMapper, ioTMapper);
	}

	public EventHubEventProcessorFactory(BrokerOptions brokerOptions,
		IMessageMapperFactory messageMapperFactory,
		IDomainEventHandlerFactoryAsync eventHandlerFactoryAsync)
	{
		var messageMapper = messageMapperFactory.CreateMessageMapper<T>();
		var domainEventHandlerAsync = eventHandlerFactoryAsync.CreateDomainEventHandlerAsync<T>();

		if (messageMapper == null)
			throw new Exception(
				$"No MessageMapper has found for {typeof(T)}. A MessageMapper must be specified for every DomainEvent");

		DomainEventProcessorAsync =
			new AzureDomainEventProcessorAsync<T>(brokerOptions, messageMapper, domainEventHandlerAsync);
	}

	public EventHubEventProcessorFactory(BrokerOptions brokerOptions,
		IMessageMapperFactory messageMapperFactory,
		IIoTMapper ioTMapper,
		IDomainEventHandlerFactoryAsync eventHandlerFactoryAsync)
	{
		var messageMapper = messageMapperFactory.CreateMessageMapper<T>();
		var domainEventHandlerAsync = eventHandlerFactoryAsync.CreateDomainEventHandlerAsync<T>();

		if (messageMapper == null)
			throw new Exception(
				$"No MessageMapper has found for {typeof(T)}. A MessageMapper must be specified for every DomainEvent");

		DomainEventProcessorAsync =
			new AzureDomainEventProcessorAsync<T>(brokerOptions, messageMapper, ioTMapper, domainEventHandlerAsync);
	}
}