using Muflone.Messages.Commands;

namespace Muflone.Mercurio.Azure.Factories;

public interface ICommandProcessorFactoryAsync
{
    ICommandProcessorAsync<T> CreateCommandProcessorAsync<T>() where T : class, ICommand;
}