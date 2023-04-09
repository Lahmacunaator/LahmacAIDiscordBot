using System.Security.Claims;
using DiscordBot.Interfaces;
using DSharpPlus.CommandsNext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LahmacAIBot.Pages;

[Authorize(AuthenticationSchemes = "Discord")]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IBotService _botService;

    public string WelcomeMessage { get; set; }
    public List<Command> Commands { get; set; }

    public IndexModel(ILogger<IndexModel> logger, IBotService botService)
    {
        _logger = logger;
        _botService = botService;
    }
    
    public async Task OnGetAsync()
    {
        var userName = User.Claims.First(c => c.Type == ClaimTypes.Name).Value;
        var bot = _botService.GetBot();
        WelcomeMessage = $"Welcome {userName}, {bot.Client.CurrentUser.Username} is up and running";

        Commands = _botService.GetCommandsList();
    }

    public async Task ExecuteCommand(string commandName, string rawParameters)
    {
        var bot = _botService.GetBot();
        var webChannel = await _botService.GetWebChannel();
        
        var command = Commands.First(c => c.Name.Equals(commandName));
        var ctx = bot.Commands.CreateFakeContext(bot.Client.CurrentUser, webChannel, "", "!", command,
            rawParameters);
        await command.ExecuteAsync(ctx);
    }

    public async Task OnPostCommandAsync(string commandName, string rawParameters)
    {
        var bot = _botService.GetBot();
        var webChannel = await _botService.GetWebChannel();
        
        var command = Commands.First(c => c.Name.Equals(commandName));
        var ctx = bot.Commands.CreateFakeContext(bot.Client.CurrentUser, webChannel, "", "!", command,
            rawParameters);
        await command.ExecuteAsync(ctx);
    }
}