using System.Diagnostics;

public interface IRestartService
{
    Task RestartApplicationAsync();
}

public class RestartService : IRestartService
{
    private readonly IHostApplicationLifetime _applicationLifetime;

    public RestartService(IHostApplicationLifetime applicationLifetime)
    {
        _applicationLifetime = applicationLifetime;
    }

    public async Task RestartApplicationAsync()
    {
        try
        {
            Console.WriteLine("Restarting the application...");

            // Get the path of the current application
            var currentAppPath = Process.GetCurrentProcess().MainModule.FileName;

            // Stop the current application
            _applicationLifetime.StopApplication();

            // Wait for the application to shut down gracefully
            await Task.Delay(10000); // Adjust the delay if needed

            // Launch a new instance of the application
            Process.Start(new ProcessStartInfo
            {
                FileName = currentAppPath,
                Arguments = string.Join(" ", Environment.GetCommandLineArgs().Skip(1)),
                UseShellExecute = true // ensures that the process starts with the default shell
            });

            Console.WriteLine("New instance of the application started.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to restart the application: {ex.Message}");
        }
    }
}
