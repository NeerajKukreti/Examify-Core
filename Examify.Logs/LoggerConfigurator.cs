using Serilog;
using Serilog.Sinks.Graylog;

public static class LoggerConfigurator
{
    public static void ConfigureLogger(string? graylogHost = null, int graylogPort = 12201, string logPath = "Logs/app-.log")
    {
        var loggerConfig = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day);


        try
        {
            var logDir = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            loggerConfig = loggerConfig.WriteTo.File(logPath, rollingInterval: RollingInterval.Day);
        }
        catch
        {
            // If file logging fails, continue with console only
        }

        if (!string.IsNullOrEmpty(graylogHost))
        {
            loggerConfig = loggerConfig.WriteTo.Graylog(new GraylogSinkOptions
            {
                HostnameOrAddress = graylogHost,
                Port = graylogPort
            });
        }

        Log.Logger = loggerConfig.CreateLogger();
    }
}
