using System;
using System.Threading;
using System.Threading.Tasks;
using Muflone.Azure.Exceptions;
using Muflone.Messages;
using Muflone.Messages.Events;

namespace Muflone.Azure.Factories;

public interface IIntegrationEventProcessorAsync
{
}

public interface IIntegrationEventProcessorAsync<in T> where T : IIntegrationEvent
{
    event EventHandler<MufloneExceptionArgs> MufloneExceptionHandler;

    void RegisterBroker();
    Task HandleAsync(Message message, CancellationToken token = default);
    Task PublishAsync(T domainEvent, CancellationToken token = default);
}