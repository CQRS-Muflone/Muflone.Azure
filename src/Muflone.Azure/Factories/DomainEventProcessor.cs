using System;
using Muflone.Messages;
using Muflone.Messages.Events;

namespace Muflone.Azure.Factories;

public class DomainEventProcessor<T> : IDomainEventProcessor<T> where T : class, IDomainEvent
{
	private readonly BrokerOptions _brokerOptions;
	private readonly IDomainEventHandler<T> _domainEventHandler;
	private readonly IMessageMapper<T> _messageMapper;

	internal DomainEventProcessor(BrokerOptions brokerOptions,
		IMessageMapper<T> messageMapper,
		IDomainEventHandler<T> eventHandler)
	{
		_brokerOptions = brokerOptions;

		_messageMapper = messageMapper;
		_domainEventHandler = eventHandler;
	}

	internal DomainEventProcessor(BrokerOptions brokerOptions,
		IMessageMapper<T> messageMapper)
	{
		_brokerOptions = brokerOptions;
		_messageMapper = messageMapper;
	}

	public event EventHandler ExceptionHandler;

	public virtual void RegisterBroker()
	{
		if (_domainEventHandler == null)
			throw new Exception(
				$"No DomainEventHandler has found for {typeof(T)}. At least one DomainEventHandler must be specified for every DomainEvent");
	}

	public virtual void Handle(Message message)
	{
		// Map the message
		var domainEvent = _messageMapper.MapToRequest(message);
		// Process the domainEvent
		_domainEventHandler.Handle(domainEvent);
	}

	public virtual void Publish(T domainEvent)
	{
	}
}