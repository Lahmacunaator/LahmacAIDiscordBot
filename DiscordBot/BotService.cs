using DiscordBot.Commands;
using DiscordBot.Interfaces;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Microsoft.Extensions.Options;

namespace DiscordBot;

public class BotService : IBotService
{
    private readonly BotConfiguration _botConfiguration;
    private Bot? _bot;

    public BotService(IOptionsMonitor<BotConfiguration> botConfiguration)
    {
        _botConfiguration = botConfiguration.CurrentValue;
    }

    public Bot CreateBot()
    {
        _bot = new Bot(_botConfiguration);
        return _bot;
    }

    public Bot? GetBot() => _bot;

    public List<Command> GetCommandsList()
    {
        var commands = _bot.Commands.RegisteredCommands;
        return commands.Select(kvPair => kvPair.Value).ToList();
    }
    
    public Dictionary<string,List<Type>> GetCommandParams()
    {
        var type = typeof(CoreCommands);
        var methods = type.GetMethods().Select(m => m).Where(m => m.Name.Contains("Command")).ToList();
        var allParamTypes = methods.Select(method => method.GetParameters().Select(info => info.ParameterType).Where(p => p != typeof(CommandContext)).Select(p => p).ToList()).ToList();
        var result = new Dictionary<string, List<Type>> { { "help", new List<Type>() } };

        for (var index = 0; index < allParamTypes.Count; index++)
        {
            var methodParams = allParamTypes[index];
            var methodName = methods[index].Name.Replace("Command", "").ToLower();
            result.Add(methodName, methodParams);
        }

        return result;
    }

    public async Task<DiscordChannel> GetWebChannel() => await _bot.Client.GetChannelAsync(_botConfiguration.WebChannelId);
}