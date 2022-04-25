using System;
using System.Threading;
using System.Threading.Tasks;
using Muflone.Mercurio.Azure.Exceptions;
using Muflone.Messages;
using Muflone.Messages.Commands;

namespace Muflone.Mercurio.Azure.Factories;

public class CommandProcessorAsync<T> : ICommandProcessorAsync<T> where T : class, ICommand
{
    public event EventHandler<MufloneExceptionArgs> MufloneExceptionHandler;

    private readonly BrokerOptions _brokerOptions;
    private readonly IMessageMapper<T> _messageMapper;
    private readonly ICommandHandlerAsync<T> _commandHandlerAsync;

    internal CommandProcessorAsync(BrokerOptions brokerOptions,
        IMessageMapper<T> messageMapper,
        ICommandHandlerAsync<T> commandHandlerAsync)
    {
        _brokerOptions = brokerOptions;
        
        _messageMapper = messageMapper;
        _commandHandlerAsync = commandHandlerAsync;
    }

    internal CommandProcessorAsync(BrokerOptions brokerOptions,
        IMessageMapper<T> messageMapper)
    {
        _brokerOptions = brokerOptions;
        _messageMapper = messageMapper;
    }

    public virtual void RegisterBroker()
    { }

    public virtual async Task HandleAsync(Message message, CancellationToken token = default(CancellationToken))
    {
        if (_commandHandlerAsync == null)
            throw new Exception($"No CommandHandler has found for {typeof(T)}. A CommandHandler must be specified for every Command");

        try
        {
            // Map the message
            var command = _messageMapper.MapToRequest(message);
            // Process the command
            await _commandHandlerAsync.HandleAsync(command, token);
        }
        catch (Exception ex)
        {
            OnExceptionHandler(new MufloneExceptionArgs(ex));
        }
    }

    public virtual Task SendAsync(T command, CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    protected virtual void OnExceptionHandler(MufloneExceptionArgs e)
    {
        var handler = MufloneExceptionHandler;
        handler?.Invoke(this, e);
    }
}