using System.Diagnostics;

namespace GPT;

public class GptService
{
    Process process = new();

    void LaunchProcess()
    {
        process.EnableRaisingEvents = true;
        process.OutputDataReceived += ProcessOutputDataReceived;
        process.ErrorDataReceived += ProcessErrorDataReceived;
        process.Exited += ProcessExited;

        process.StartInfo.FileName = "some.exe";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardOutput = true;

        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        //below line is optional if we want a blocking call
        process.WaitForExit();
    }

    void ProcessExited(object? sender, EventArgs e)
    {
        Console.WriteLine($"process exited with code {process.ExitCode.ToString()}\n");
    }

    void ProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        Console.WriteLine(e.Data + "\n");
    }

    void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        Console.WriteLine(e.Data + "\n");
    }

    public void InputGpt(string input)
    {
        process.StandardInput.WriteLine(input);
    }
}