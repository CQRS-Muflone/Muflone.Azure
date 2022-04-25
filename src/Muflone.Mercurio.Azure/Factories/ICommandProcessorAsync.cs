using System;
using System.Threading;
using System.Threading.Tasks;
using Muflone.Mercurio.Azure.Exceptions;
using Muflone.Messages;
using Muflone.Messages.Commands;

namespace Muflone.Mercurio.Azure.Factories;

public interface ICommandProcessorAsync
{ }

public interface ICommandProcessorAsync<in T> where T : ICommand
{
    event EventHandler<MufloneExceptionArgs> MufloneExceptionHandler;

    void RegisterBroker();

    Task HandleAsync(Message message, CancellationToken cancellationToken = default);

    Task SendAsync(T command, CancellationToken cancellationToken = default);
}