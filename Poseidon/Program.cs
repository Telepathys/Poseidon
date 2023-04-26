using System.Collections.Concurrent;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Poseidon;

class Program
{
    public static readonly IConfigurationRoot configuration = new ConfigurationBuilder().AddJsonFile($"{Directory.GetCurrentDirectory()}/appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", false, true).Build();
    public static readonly Logger logger = new Logger(configuration);
    public static readonly HttpContext httpContext = new HttpContextAccessor().HttpContext;
    public static ConcurrentDictionary<string, string> currenGroup = new ConcurrentDictionary<string, string>();
    public static ConcurrentDictionary<string, ConcurrentDictionary<string, WebSocket>> group = new ConcurrentDictionary<string, ConcurrentDictionary<string, WebSocket>>();
    public static readonly Socket socket = new Socket();
    public static readonly SystemMessage systemMessage = new SystemMessage();
    
    static void Main(string[] args)
    {
        DotNetEnv.Env.TraversePath().Load();
        logger.Info("Server Start");
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();
        var webSocketOptions = new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromMinutes(2)
        };
        app.UseWebSockets(webSocketOptions);
        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await socket.Start(webSocket, context);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                }
            }
            else
            {
                await next();
            }
        });
        app.Run();
    }
}