using System;
using Muflone.Factories;
using Muflone.Mercurio.Azure.ServiceBus;
using Muflone.Messages.Commands;

namespace Muflone.Mercurio.Azure.Factories;

public class ServiceBusCommandProcessorFactory<T> where T : class, ICommand
{
    public readonly ICommandProcessorAsync<T> CommandProcessorAsync;

    public ServiceBusCommandProcessorFactory(BrokerOptions brokerOptions,
        IMessageMapperFactory messageMapperFactory)
    {
        var messageMapper = messageMapperFactory.CreateMessageMapper<T>();

        if (messageMapper == null)
            throw new Exception(
                $"No MessageMapper has found for {(object)typeof(T)}. A MessageMapper must be specified for every Command");

        CommandProcessorAsync = new AzureCommandProcessorAsync<T>(brokerOptions, messageMapper);
    }

    public ServiceBusCommandProcessorFactory(BrokerOptions brokerOptions)
    {
        CommandProcessorAsync = new AzureCommandProcessorAsync<T>(brokerOptions);
    }

    public ServiceBusCommandProcessorFactory(BrokerOptions brokerOptions,
        IMessageMapperFactory messageMapperFactory,
        ICommandHandlerFactoryAsync commandHandlerFactoryAsync)
    {
        var messageMapper = messageMapperFactory.CreateMessageMapper<T>();
        var commandHandlerAsync = commandHandlerFactoryAsync.CreateCommandHandlerAsync<T>();

        if (messageMapper == null)
            throw new Exception(
                $"No MessageMapper has found for {(object)typeof(T)}. A MessageMapper must be specified for every Command");

        if (commandHandlerAsync == null)
            throw new Exception(
                $"No CommandHandler has found for {(object)typeof(T)}. A CommandHandler must be specified for every Command");

        CommandProcessorAsync = new AzureCommandProcessorAsync<T>(brokerOptions, messageMapper, commandHandlerAsync);
    }

    public ServiceBusCommandProcessorFactory(BrokerOptions brokerOptions,
        ICommandHandlerFactoryAsync commandHandlerFactoryAsync)
    {
        var commandHandlerAsync = commandHandlerFactoryAsync.CreateCommandHandlerAsync<T>();

        if (commandHandlerAsync == null)
            throw new Exception(
                $"No CommandHandler has found for {(object)typeof(T)}. A CommandHandler must be specified for every Command");

        CommandProcessorAsync = new AzureCommandProcessorAsync<T>(brokerOptions, commandHandlerAsync);
    }

    // TODO: fa figo ;-) ma lo faremo
    //public ICommandProcessorAsync<T> Build()
    //{
    //    return new AzureCommandProcessorAsync<T>(brokerOptions, messageMapper, commandHandlerAsync);
    //}
}