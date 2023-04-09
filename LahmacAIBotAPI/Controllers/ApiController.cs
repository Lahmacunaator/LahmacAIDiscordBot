using System.Security.Claims;
using DiscordBot.Interfaces;
using DiscordBot.WebCommands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace LahmacAIBot.Controllers;

[Authorize(AuthenticationSchemes = "Discord")]
[Route("[Controller]")]
public class ApiController : ControllerBase
{
    private readonly IBotService _botService;

    public ApiController(IBotService botService)
    {
        _botService = botService;
    }

    public ActionResult Index()
    {
        //var redirect = Redirect("https://localhost:7179/");
        //return redirect;
        return Ok();
    }

    [HttpGet("user")]
    public string GetUser()
    {
        var id = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        var userName = User.Claims.First(c => c.Type == ClaimTypes.Name).Value;
        
        return $"welcome {userName}, your id is {id}";
    }

    [DisableCors]
    [HttpPost("command")]
    public async Task<IActionResult> ExecuteCommand(CommandInfo commandInfo)
    {
        var bot = _botService.GetBot();
        var webChannel = await _botService.GetWebChannel();
        
        var command = _botService.GetCommandsList().First(c => c.Name.Equals(commandInfo.Name));
        var ctx = bot.Commands.CreateFakeContext(bot.Client.CurrentUser, webChannel, "", "!", command,
            commandInfo.RawParameters);
        await command.ExecuteAsync(ctx);
        return Accepted();
    }

}