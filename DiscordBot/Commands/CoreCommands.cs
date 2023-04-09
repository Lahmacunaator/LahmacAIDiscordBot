using DiscordBot.Audio;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.VoiceNext;

namespace DiscordBot.Commands;

public class CoreCommands : BaseCommandModule
{
    private readonly AudioService _audioService;

    public CoreCommands()
    {
        _audioService = new AudioService();
    }

    [Command("ping")]
    public async Task PingCommand(CommandContext context)
    {
        Console.WriteLine("triggered Command");
        await context.RespondAsync($"Pong! {context.Client.Ping} ms");
    }

    [Command("add")]
    public async Task AddCommand(CommandContext context, int firstNumber, int secondNumber)
    {
        await context.RespondAsync($"{firstNumber} + {secondNumber} = {firstNumber + secondNumber}");
    }

    [Command("kedi")]
    public async Task KediCommand(CommandContext context)
    {
        await context.RespondAsync($"Nyx");
    }
    
    [Command("join")]
    public async Task JoinCommand(CommandContext ctx)
    {
        var channel = ctx.Member.VoiceState?.Channel;

        await channel.ConnectAsync();
    }

    [Command("play")]
    public async Task PlayCommand(CommandContext ctx, string filename = "VineBoom")
    {
        var vnext = ctx.Client.GetVoiceNext();
        var connection = vnext.GetConnection(ctx.Guild);
        var pcm = _audioService.GetAudioStream(filename);
        
        var transmit = connection.GetTransmitSink();
        await _audioService.AudioQueue.Enqueue(() => pcm.CopyToAsync(transmit));
    }

    [Command("leave")]
    public async Task LeaveCommand(CommandContext ctx)
    {
        var vnext = ctx.Client.GetVoiceNext();
        var connection = vnext.GetConnection(ctx.Guild);

        connection.Disconnect();
    }
}