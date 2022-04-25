using Muflone.Messages.Commands;
using Muflone.Messages.Events;

namespace Muflone.Mercurio.Azure.Abstracts;

public interface IRegisterHandlersAsync
{
    void RegisterCommandHandler<T>() where T : class, ICommand;
    void RegisterDomainEventHandler<T>() where T : class, IDomainEvent;
    void RegisterIntegrationEventHandler<T>() where T : class, IIntegrationEvent;

    void MufloneExceptionHandler(object sender, Exceptions.MufloneExceptionArgs e);

    string Message { get; }
    string StackTrace { get; }
    string Source { get; }
}