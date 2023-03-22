using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;

namespace LahmacAIBot;

public class Bot
{
    public DiscordClient Client { get; private set; }
    public InteractivityExtension Interactivity { get; private set; }
    public CommandsNextExtension Commands { get; private set; }

    public async Task RunAsync()
    {
        
    }
}