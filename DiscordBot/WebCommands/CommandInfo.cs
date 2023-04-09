namespace DiscordBot.WebCommands;

public class CommandInfo
{
    public string Name { get; set; }
    public List<Type>? ParameterTypes { get; set; }
    public string RawParameters { get; set; }
}