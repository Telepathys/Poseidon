using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Poseidon;

public class Startup
{
    public void Configure(IApplicationBuilder app)
    {
        app.UseErrorMiddleware();
    }
}