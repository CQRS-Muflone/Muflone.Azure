using System;
using Muflone.Messages;
using Muflone.Messages.Commands;

namespace Muflone.Azure.Factories;

public class CommandProcessor<T> : ICommandProcessor<T> where T : class, ICommand
{
	private readonly BrokerOptions _brokerOptions;
	private readonly ICommandHandler<T> _commandHandler;
	private readonly IMessageMapper<T> _messageMapper;

	internal CommandProcessor(BrokerOptions brokerOptions,
		IMessageMapper<T> messageMapper,
		ICommandHandler<T> commandHandler)
	{
		_brokerOptions = brokerOptions;

		_messageMapper = messageMapper;
		_commandHandler = commandHandler;
	}

	internal CommandProcessor(BrokerOptions brokerOptions,
		IMessageMapper<T> messageMapper)
	{
		_brokerOptions = brokerOptions;

		_messageMapper = messageMapper;
	}

	public event EventHandler ExceptionHandler;

	public virtual void RegisterBroker()
	{
		if (_commandHandler == null)
			throw new Exception(
				$"No CommandHandler has found for {typeof(T)}. A CommandHandler must be specified for every Command");
	}

	public virtual void Handle(Message message)
	{
		// Map the message
		var command = _messageMapper.MapToRequest(message);
		// Process the command
		_commandHandler.Handle(command);
	}

	public virtual void Send(T command)
	{
	}
}