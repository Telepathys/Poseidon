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
    public static readonly Socket socket = new Socket();
    public static readonly SystemMessage systemMessage = new SystemMessage();
    public static readonly MessageLimit messageLimit = new MessageLimit();
    public static readonly Router Router = new Router();
    
    static void Main(string[] args)
    {
        DotNetEnv.Env.TraversePath().Load();
        logger.Info("Server Start");
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddTransient<ErrorHandlingMiddleware>();
        var app = builder.Build();
        var webSocketOptions = new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromMinutes(2)
        };
        app.UseWebSockets(webSocketOptions);
        app.UseGlobalExceptionHandler();
        app.Scheduler(() =>
        {
            new Scheduler().Start();
        });
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
                await next.Invoke();
            }
        });
        
        app.Run();
    }
}