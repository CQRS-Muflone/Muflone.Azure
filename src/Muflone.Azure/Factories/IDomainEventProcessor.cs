using System;
using Muflone.Messages;
using Muflone.Messages.Events;

namespace Muflone.Azure.Factories;

public interface IDomainEventProcessor
{
}

public interface IDomainEventProcessor<in T> where T : IDomainEvent
{
	event EventHandler ExceptionHandler;

	void RegisterBroker();
	void Handle(Message message);
	void Publish(T domainEvent);
}