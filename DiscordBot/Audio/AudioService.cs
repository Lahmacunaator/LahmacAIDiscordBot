using System.Diagnostics;
using NAudio.Wave;

namespace DiscordBot.Audio;

public class AudioService
{
    public AudioQueue AudioQueue;

    private AudioServiceConfiguration Configuration = new()
    {
        AudioFolderPath = "E:\\LahmacAIDiscordBot\\LahmacAIBot\\DiscordBot\\Audio\\Files",
        FFMPEGPath = "E:\\ffmpeg-6.0-essentials_build\\bin\\ffmpeg.exe"
    };

    public AudioService()
    {
        AudioQueue = new AudioQueue();
    }

    public Stream GetAudioStream(string fileName)
    {
        var filePath = $"{Configuration.AudioFolderPath}\\{fileName}.mp3";//E:\\LahmacAIDiscordBot\\LahmacAIBot\\DiscordBot\\Audio\\VineBoom.mp3";
        var ffmpeg = Process.Start(new ProcessStartInfo
        {
            FileName = Configuration.FFMPEGPath, //"E:\\ffmpeg-6.0-essentials_build\\bin\\ffmpeg.exe",
            Arguments = $@"-i ""{filePath}"" -ac 2 -f s16le -ar 48000 pipe:1",
            RedirectStandardOutput = true,
            UseShellExecute = false
        });

        return ffmpeg.StandardOutput.BaseStream;
    }

    public List<AudioData> GetAllAudioData() => Directory.EnumerateFiles(Configuration.AudioFolderPath, "*.mp3")
        .Select(fileName => new AudioData
        {
            AudioName = fileName.Split('\\').Last().Replace(".mp3", ""),
            Duration = new Mp3FileReader(fileName).TotalTime.TotalMilliseconds
        }).ToList();

    public string GetAudioStoragePath()
    {
        return Configuration.AudioFolderPath;
    }
}