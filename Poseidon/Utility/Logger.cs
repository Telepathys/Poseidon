using System.Collections.Concurrent;
using System.Net.WebSockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlackLogger;

namespace Poseidon;

public class Logger
{
    private readonly ILogger _logger;
    private readonly ErrorMessage _errorMessage = new ErrorMessage();
    public Logger(ILogger<Logger> logger)
    {
        _logger = logger;
    }
    
    public Logger(IConfiguration configuration)
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton(configuration)
            .AddLogging(builder =>
                {
                    builder.AddConfiguration(configuration.GetSection("Logging"));
                    builder.AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = true;
                        options.SingleLine = true;
                        options.TimestampFormat = "HH:mm:ss ";
                    }).AddSlack();
                }
            ).BuildServiceProvider();
        _logger = serviceProvider.GetService<ILogger<Logger>>();
    }

    public void Debug(string message)
    {
        _logger.LogDebug(message);
    }
    public void Info(string message)
    {
        _logger.LogInformation(message);
    }
    public void Warn(string message)
    {
        _logger.LogWarning(message);
    }
    public void Error(string message, ConcurrentDictionary<User, WebSocket>? webSockets = null, User? user = null)
    {
        _logger.LogError(message);
        if(webSockets != null && user != null)
            _errorMessage.Send(user, webSockets, message);
    }
    public void Critical(string message)
    {
        _logger.LogCritical(message);
    }
}