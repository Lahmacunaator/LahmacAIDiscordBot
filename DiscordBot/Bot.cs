using DiscordBot.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.VoiceNext;

namespace DiscordBot;

public class Bot
{
    private readonly BotConfiguration _configuration;
    public DiscordClient Client { get; private set; }
    public InteractivityExtension Interactivity { get; private set; }
    public CommandsNextExtension Commands { get; private set; }

    public Bot(BotConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task RunAsync()
    {
        var config = new DiscordConfiguration
        {
            Intents = DiscordIntents.All,
            Token = _configuration.Token,
            TokenType = TokenType.Bot,
            AutoReconnect = true
        };

        Client = new DiscordClient(config);
        Client.UseVoiceNext();
        Client.UseInteractivity(new InteractivityConfiguration
        {
            Timeout = TimeSpan.FromMinutes(2),
        });

        //Client.Ready += OnClientReady();
        
        Client.VoiceStateUpdated += ClientOnVoiceStateUpdated;
        
        var commandsConfig = new CommandsNextConfiguration
        {
            StringPrefixes = new []{ _configuration.Prefix },
            EnableMentionPrefix = true,
            EnableDms = true,
            CaseSensitive = false,
            EnableDefaultHelp = true
        };

        Commands = Client.UseCommandsNext(commandsConfig);
        Commands.RegisterCommands<CoreCommands>();
        
        await Client.ConnectAsync();
    }

    private Task ClientOnVoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs args)
    {
        Console.WriteLine("Voice State Changed");
        return Task.CompletedTask;
    }

    private Task OnClientReady(ReadyEventArgs e)
    {
        Console.WriteLine(e);
        
        return Task.CompletedTask;
    }
}