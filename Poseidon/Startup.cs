using System.Net;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Poseidon;

public static class ApplicationBuilderExtensions
{
    // 최초 한번 실행되는 미들웨어
    public static IApplicationBuilder Scheduler(this IApplicationBuilder app, Action action)
    {
        action(); 
        return app.Use(next => async context =>
        {
            await next(context);
        });
    }
}

public class ErrorHandlingMiddleware: IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            Program.logger.Error(e.Message);
        }
    }
}

public static class Startup
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        => app.UseMiddleware<ErrorHandlingMiddleware>();
}

// // GRPC 요청 인터셉터
// public class AuthInterceptor : Interceptor
// {
//     private readonly JwtTokenSystem jwtTokenSystem;
//
//     public AuthInterceptor(JwtTokenSystem jwtTokenSystem)
//     {
//         this.jwtTokenSystem = jwtTokenSystem;
//     }
//
//     public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
//         TRequest request,
//         ServerCallContext context,
//         UnaryServerMethod<TRequest, TResponse> continuation)
//     {
//         var authorization = context.RequestHeaders.Get("Authorization");
//         User user = jwtTokenSystem.ValidateJwtToken(authorization.Value.Replace("Bearer ", ""));
//
//         if (user == null)
//         {
//             Program.logger.Error("gRPC Token is invalid");
//             throw new RpcException(new Status(StatusCode.Unauthenticated, "gRPC Token is invalid"));
//         }
//
//         return await continuation(request, context);
//     }
// }