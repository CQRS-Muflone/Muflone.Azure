using Muflone.Messages.Commands;

namespace Muflone.Mercurio.Azure.Factories;

public interface ICommandHandlerFactory
{
    ICommandHandler<T> CreateCommandHandler<T>() where T : class, ICommand;
}