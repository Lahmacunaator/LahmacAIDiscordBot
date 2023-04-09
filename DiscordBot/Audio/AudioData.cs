namespace DiscordBot.Audio;

public class AudioData
{
    public string AudioName { get; set; }
    public double Duration { get; set; }
    public double Progress { get; set; }
    public ButtonType ButtonType { get; set; } = ButtonType.Primary;
}

public enum ButtonType
{
    Primary,
    Success,
    Disabled
}