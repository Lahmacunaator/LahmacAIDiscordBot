using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace DiscordBot.Interfaces;

public interface IBotService
{
    Bot CreateBot();
    Bot? GetBot();
    List<Command> GetCommandsList();
    Dictionary<string, List<Type>> GetCommandParams();
    Task<DiscordChannel> GetWebChannel();
}