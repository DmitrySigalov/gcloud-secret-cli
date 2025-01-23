using Google.Cloud.SecretManager.Client.Commands.Handlers;
using Google.Cloud.SecretManager.Client.Common;
using Google.Cloud.SecretManager.Client.Profiles;
using Google.Cloud.SecretManager.Client.UserRuntime;
using Sharprompt;

namespace Google.Cloud.SecretManager.Client.Commands;

public class CommandSelector
{
    private readonly UserParameters _userParameters;
    private readonly IProfileConfigProvider _profileConfigProvider;
    
    private readonly ICommandHandler _helpCommandHandler;
    private readonly ICommandHandler _configCommandHandler;
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

        _configCommandHandler = handlers.Single(x => x.CommandName == ConfigProfileCommandHandler.COMMAND_NAME);
        _defaultCommandHandler = handlers.Single(x => x.CommandName == SetEnvCommandHandler.COMMAND_NAME);
    }
    
    public ICommandHandler Get()
    {
        var commandName = _userParameters.CommandName;
        
        if (commandName=="*" || string.IsNullOrEmpty(commandName))
        {
            if (_profileConfigProvider.GetNames().Any())
            {
                commandName = Prompt.Select(
                    "Select command",
                    _allCommandHandlers.Select(x => x.Key),
                    defaultValue: _defaultCommandHandler?.CommandName);
            }
            else
            {
                commandName = _configCommandHandler.CommandName;
            }
        }

        if (!_allCommandHandlers.TryGetValue(commandName, out var handler))
        {
            ConsoleHelper.WriteLineError("Invalid command argument");
            Console.WriteLine();
            
            return _helpCommandHandler;
        }
        
        return handler;
    }
}