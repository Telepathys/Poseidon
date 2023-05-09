using System.Net;
using System.Net.WebSockets;
using Grpc.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Poseidon;


class Program
{
    public static readonly IConfigurationRoot configuration = new ConfigurationBuilder().AddJsonFile($"{Directory.GetCurrentDirectory()}/appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", false, true).Build();
    public static readonly Logger logger = new Logger(configuration);
    public static readonly Socket socket = new Socket();
    public static readonly SystemMessage systemMessage = new SystemMessage();
    public static readonly MessageLimit messageLimit = new MessageLimit();
    public static readonly Router Router = new Router();
    
    static void Main(string[] args)
    {
        DotNetEnv.Env.TraversePath().Load();
        logger.Info("Server Start");
        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.ConfigureKestrel(options =>
        {
            // 웹 소켓 포트
            options.Listen(IPAddress.Any, 33333, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http1;
            });
            // gROC 포트
            options.Listen(IPAddress.Any, 33334, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
            });
        });
        builder.Services.AddTransient<ErrorHandlingMiddleware>();
        builder.Services.AddGrpc();
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
        
        app.MapGrpcService<PoseidonGrpcServer>().RequireHost("localhost:33334");

        app.Run();
    }
}