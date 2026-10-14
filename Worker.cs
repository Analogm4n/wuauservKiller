using System.Diagnostics;

namespace ServiceKiller;

public class Worker : BackgroundService
{
    private readonly string _targetService = "wuauserv";
    private readonly string _logPath = @"C:\ProgramData\ServiceKiller\service-killer.log";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        EnsureLogDirectory();
        Log("Service started. Cycle every 5 seconds.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                RunScCommand($"stop {_targetService}");
                RunScCommand($"config {_targetService} start= disabled");
            }
            catch (Exception ex)
            {
                Log($"ERROR in the cycle: {ex.Message}");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Normal cancellation upon stopping the service
            }
        }

        Log("Service stopped.");
    }

    private void RunScCommand(string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "sc.exe",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process == null)
        {
            Log($"Failed to start the process: sc {arguments}");
            return;
        }

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        Log($"Command: sc {arguments} | ExitCode: {process.ExitCode}");
        if (!string.IsNullOrWhiteSpace(output))
            Log($"  STDOUT: {output.Trim()}");
        if (!string.IsNullOrWhiteSpace(error))
            Log($"  STDERR: {error.Trim()}");
    }

    private void EnsureLogDirectory()
    {
        var dir = Path.GetDirectoryName(_logPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    private void Log(string message)
    {
        string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
        try
        {
            File.AppendAllText(_logPath, line + Environment.NewLine);
        }
        catch
        {
            // If logging to disk fails, we avoid crashing the service
        }
    }
}
