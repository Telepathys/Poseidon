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