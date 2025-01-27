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

    private readonly IDictionary<string, ICommandHandler> _allCommandHandlersByFullNames;
    private readonly IDictionary<string, ICommandHandler> _allCommandHandlersByShortNames;

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

        _allCommandHandlersByFullNames = new [] { helpCommandHandler, } 
            .Union(handlers)
            .ToDictionary(
                h => h.CommandName, 
                h => h);

        _allCommandHandlersByShortNames = _allCommandHandlersByFullNames
            .Values
            .ToDictionary(
                h => h.ShortName, 
                h => h);

        _defaultCommandHandler = handlers.Single(x => x.CommandName == GetSecretsHandler.COMMAND_NAME);
    }
    
    public ICommandHandler Get(ContinueStatusEnum continueStatus)
    {
        if (continueStatus is ContinueStatusEnum.SelectCommand)
        {
            var commandName = _userParameters.CommandName;

            if (commandName == "*" || string.IsNullOrEmpty(commandName))
            {
                if (!_profileConfigProvider.GetNames().Any())
                {
                    return GetTypedCommandHandler<CreateProfileCommandHandler>();
                }

                commandName = Prompt.Select(
                    "Select command",
                    _allCommandHandlersByFullNames.Select(x => x.Key),
                    defaultValue: _defaultCommandHandler?.CommandName);
            }

            if (!_allCommandHandlersByFullNames.TryGetValue(commandName, out var handler) &&
                !_allCommandHandlersByShortNames.TryGetValue(commandName, out handler))
            {
                ConsoleHelper.WriteLineError($"Invalid command argument: '{commandName}'");
                Console.WriteLine();
            
                return _helpCommandHandler;
            }

            return handler;
        }

        if (continueStatus is ContinueStatusEnum.ConfigProfile)
        {
            return GetTypedCommandHandler<EditProfileCommandHandler>();
        }
            
        if (continueStatus is ContinueStatusEnum.SetEnvironment)
        {
            return GetTypedCommandHandler<SetEnvVarCommandHandler>();
        }

        throw new NotSupportedException();
    }

    private ICommandHandler GetTypedCommandHandler<THandler>() 
        where THandler : ICommandHandler =>
            _allCommandHandlersByFullNames.Values
                .OfType<THandler>()
                .Single();
}