using System;
using System.Threading;
using System.Threading.Tasks;
using Muflone.Azure.Exceptions;
using Muflone.Messages;
using Muflone.Messages.Events;

namespace Muflone.Azure.Factories;

public interface IDomainEventProcessorAsync
{
}

public interface IDomainEventProcessorAsync<in T> where T : IDomainEvent
{
	event EventHandler<MufloneExceptionArgs> MufloneExceptionHandler;

	void RegisterBroker();
	Task HandleAsync(Message message, CancellationToken token = default);
	Task PublishAsync(T domainEvent, CancellationToken token = default);
}