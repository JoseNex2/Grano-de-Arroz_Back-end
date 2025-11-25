using DataAccess.SupportServices;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

namespace GDA.Middleware
{
    public static class UtilitiesInversionOfControl
    {
        public static IServiceCollection AddDependency(this IServiceCollection services, WebApplicationBuilder builder, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
            builder.Host.UseSerilog();
            return services;
        }
    }
}