using System;
using Muflone.Azure.IotHub;
using Muflone.Factories;
using Muflone.Messages.Events;

namespace Muflone.Azure.Factories;

public class IoTEventProcessorFactory<T> where T : class, IDomainEvent
{
	public readonly IDomainEventProcessorAsync<T> DomainEventProcessorAsync;

	public IoTEventProcessorFactory(BrokerOptions brokerOptions,
		IMessageMapperFactory messageMapperFactory)
	{
		var messageMapper = messageMapperFactory.CreateMessageMapper<T>();

		if (messageMapper == null)
			throw new Exception(
				$"No MessageMapper has found for {typeof(T)}. A MessageMapper must be specified for every DomainEvent");

		DomainEventProcessorAsync =
			new AzureDomainEventProcessorAsync<T>(brokerOptions, messageMapper);
	}

	public IoTEventProcessorFactory(BrokerOptions brokerOptions)
	{
		DomainEventProcessorAsync =
			new AzureDomainEventProcessorAsync<T>(brokerOptions);
	}

	public IoTEventProcessorFactory(BrokerOptions brokerOptions,
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

	public IoTEventProcessorFactory(BrokerOptions brokerOptions,
		IIoTMapper ioTMapper)
	{
		DomainEventProcessorAsync =
			new AzureDomainEventProcessorAsync<T>(brokerOptions, ioTMapper);
	}
}