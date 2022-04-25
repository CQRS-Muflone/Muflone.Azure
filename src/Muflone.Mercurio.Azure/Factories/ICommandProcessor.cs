using System;
using Muflone.Messages;
using Muflone.Messages.Commands;

namespace Muflone.Mercurio.Azure.Factories;

public interface ICommandProcessor
{ }

public interface ICommandProcessor<in T> where T : ICommand
{
    event EventHandler ExceptionHandler;

    void RegisterBroker();
    void Handle(Message message);

    void Send(T command);
}