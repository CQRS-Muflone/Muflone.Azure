using System;
using Muflone.Factories;
using Muflone.Messages.Commands;

namespace Muflone.Azure.Factories;

public class CommandProcessorFactory<T> where T : class, ICommand
{
	public readonly ICommandProcessor<T> CommandProcessor;
	public readonly ICommandProcessorAsync<T> CommandProcessorAsync;

	#region Async

	public CommandProcessorFactory(BrokerOptions brokerOptions,
		IMessageMapperFactory messageMapperFactory,
		ICommandHandlerFactoryAsync commandHandlerFactoryAsync)
	{
		var messageMapper = messageMapperFactory.CreateMessageMapper<T>();
		var commandHandlerAsync = commandHandlerFactoryAsync.CreateCommandHandlerAsync<T>();

		if (messageMapper == null)
			throw new Exception(
				$"No MessageMapper has found for {typeof(T)}. A MessageMapper must be specified for every Command");

		CommandProcessorAsync = new CommandProcessorAsync<T>(brokerOptions, messageMapper, commandHandlerAsync);
	}

	#endregion

	#region Sync

	public CommandProcessorFactory(BrokerOptions brokerOptions,
		IMessageMapperFactory messageMapperFactory,
		ICommandHandlerFactory commandHandlerFactory)
	{
		var messageMapper = messageMapperFactory.CreateMessageMapper<T>();
		var commandHandler = commandHandlerFactory.CreateCommandHandler<T>();

		if (messageMapper == null)
			throw new Exception(
				$"No MessageMapper has found for {typeof(T)}. A MessageMapper must be specified for every Command");

		CommandProcessor = new CommandProcessor<T>(brokerOptions, messageMapper, commandHandler);
	}

	public CommandProcessorFactory(BrokerOptions brokerOptions,
		IMessageMapperFactory messageMapperFactory)
	{
		var messageMapper = messageMapperFactory.CreateMessageMapper<T>();

		if (messageMapper == null)
			throw new Exception(
				$"No MessageMapper has found for {typeof(T)}. A MessageMapper must be specified for every Command");

		CommandProcessor = new CommandProcessor<T>(brokerOptions, messageMapper);
		CommandProcessorAsync = new CommandProcessorAsync<T>(brokerOptions, messageMapper);
	}

	#endregion
}