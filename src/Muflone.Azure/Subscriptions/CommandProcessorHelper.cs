using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Muflone.Azure.Factories;
using Muflone.Factories;
using Muflone.Messages.Commands;

namespace Muflone.Azure.Subscriptions;

public static class CommandProcessorHelper
{
    public static IServiceCollection AddCommandProcessor<T>(this IServiceCollection services,
        string connectionString) where T : Command
    {
        services.AddScoped(provider =>
        {
            var commandHandlerFactory = provider.GetService<ICommandHandlerFactoryAsync>();

            var brokerOptions = new BrokerOptions
            {
                ConnectionString = connectionString,
                QueueName = nameof(T).ToLower()
            };

            return commandHandlerFactory is null
                ? RegisterCommandProcessorWithoutCommandHandler<T>(brokerOptions)
                : RegisterCommandProcessorWithCommandHandler<T>(brokerOptions, commandHandlerFactory);
        });

        return services;
    }

    private static ICommandProcessorAsync<T> RegisterCommandProcessorWithCommandHandler<T>(BrokerOptions brokerOptions,
        ICommandHandlerFactoryAsync commandHandlerFactory) where T : Command
    {
        var commandProcessorFactory =
            new ServiceBusCommandProcessorFactory<T>(brokerOptions, commandHandlerFactory);
        return commandProcessorFactory.CommandProcessorAsync;
    }

    private static ICommandProcessorAsync<T> RegisterCommandProcessorWithoutCommandHandler<T>(BrokerOptions brokerOptions) where T : Command
    {
        var commandProcessorFactory =
            new ServiceBusCommandProcessorFactory<T>(brokerOptions);
        return commandProcessorFactory.CommandProcessorAsync;
    }
}