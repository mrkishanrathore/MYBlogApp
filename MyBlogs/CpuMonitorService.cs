using System.Diagnostics;

public class CpuMonitorService : BackgroundService
{
    private readonly int _cpuUsageLimit = 70; // 70% CPU usage limit
    private readonly IRestartService _restartService;
    private bool _isRestarting = false; // Indicates whether a restart is in progress

    public CpuMonitorService(IRestartService restartService)
    {
        _restartService = restartService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && !_isRestarting)
        {
            var cpuUsage = GetCpuUsage();
            Console.WriteLine($"CPU Usage: {cpuUsage}%");

            if (cpuUsage > _cpuUsageLimit && !_isRestarting)
            {
                _isRestarting = true;
                await _restartService.RestartApplicationAsync();
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Check every 10 seconds
        }
    }

    private double GetCpuUsage()
    {
        using (var performanceCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total"))
        {
            performanceCounter.NextValue(); // Warm up the counter
            Thread.Sleep(500); // Small delay for accurate reading
            return performanceCounter.NextValue();
        }
    }
}
