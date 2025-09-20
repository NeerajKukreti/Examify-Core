using Serilog;
using Serilog.Sinks.Graylog;

public static class LoggerConfigurator
{
    public static void ConfigureLogger(string? graylogHost = null, int graylogPort = 12201, string logPath = "Logs/app-.log")
    {
        var loggerConfig = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day);

        if (!string.IsNullOrEmpty(graylogHost))
        {
            loggerConfig = loggerConfig.WriteTo.Graylog(new GraylogSinkOptions
            {
                HostnameOrAddress = graylogHost,
                Port = graylogPort
                // TransportType property removed; use default transport
            });
        }

        Log.Logger = loggerConfig.CreateLogger();
    }
}
