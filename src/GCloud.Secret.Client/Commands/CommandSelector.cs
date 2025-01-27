using GCloud.Secret.Client.Commands.Handlers;
using GCloud.Secret.Client.Commands.Handlers.ProfileConfiguration;
using GCloud.Secret.Client.Commands.Handlers.EnvironmentVariables;
using GCloud.Secret.Client.Commands.Handlers.Secrets;
using GCloud.Secret.Client.Common;
using GCloud.Secret.Client.Profiles;
using GCloud.Secret.Client.UserRuntime;
using Sharprompt;

namespace GCloud.Secret.Client.Commands;

public class CommandSelector
{
    private readonly UserParameters _userParameters;
    private readonly IProfileConfigProvider _profileConfigProvider;
    
    private readonly ICommandHandler _helpCommandHandler;
    private readonly ICommandHandler _defaultCommandHandler;

    private readonly IDictionary<string, ICommandHandler> _allCommandHandlers;

    public CommandSelector(
        UserParameters userParameters,
        IProfileConfigProvider profileConfigProvider,
        IEnumerable<ICommandHandler> handlers,
        HelpCommandHandler helpCommandHandler)
    {
        _userParameters = userParameters;
        _profileConfigProvider = profileConfigProvider;
        
        handlers = handlers.ToArray();
        
        _helpCommandHandler = helpCommandHandler;

        _allCommandHandlers = new [] { helpCommandHandler, } 
            .Union(handlers)
            .ToDictionary(h => h.CommandName, h => h);

        _defaultCommandHandler = handlers.Single(x => x.CommandName == GetSecretsHandler.COMMAND_NAME);
    }
    
    public ICommandHandler Get(ContinueStatusEnum continueStatus)
    {
        if (continueStatus is ContinueStatusEnum.SelectCommand)
        {
            if (!_profileConfigProvider.GetNames().Any())
            {
                return GetTypedCommandHandler<CreateProfileCommandHandler>();
            }

            var commandName = _userParameters.CommandName;

            if (commandName == "*" || string.IsNullOrEmpty(commandName))
            {
                commandName = Prompt.Select(
                    "Select command",
                    _allCommandHandlers.Select(x => x.Key),
                    defaultValue: _defaultCommandHandler?.CommandName);
            }

            if (!_allCommandHandlers.TryGetValue(commandName, out var handler))
            {
                ConsoleHelper.WriteLineError("Invalid command argument");
                Console.WriteLine();
            
                return _helpCommandHandler;
            }

            return handler;
        }

        if (continueStatus is ContinueStatusEnum.ConfigProfile)
        {
            return GetTypedCommandHandler<ConfigProfileCommandHandler>();
        }
            
        if (continueStatus is ContinueStatusEnum.SetEnvironment)
        {
            return GetTypedCommandHandler<SetEnvCommandHandler>();
        }

        throw new NotSupportedException();
    }

    private ICommandHandler GetTypedCommandHandler<THandler>() 
        where THandler : ICommandHandler =>
            _allCommandHandlers.Values
                .OfType<THandler>()
                .Single();
}