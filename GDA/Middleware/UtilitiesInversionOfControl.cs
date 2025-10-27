using Microsoft.IdentityModel.Tokens;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog;
using System.Text;
using Utilities;

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
            services.AddAuthentication(config =>
            {
                config.DefaultAuthenticateScheme = "AccessScheme";
                config.DefaultChallengeScheme = "AccessScheme";
            }).AddJwtBearer("AccessScheme", config =>
            {
                config.RequireHttpsMetadata = false;
                config.SaveToken = true;
                config.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY_ACCESS")))
                };
            }).AddJwtBearer("RecoveryScheme", config =>
            {
                config.RequireHttpsMetadata = false;
                config.SaveToken = true;
                config.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY_RECOVERY")))
                };
            });
            services.AddSingleton<AuthenticationService>();
            services.AddScoped<IStorageService, StorageService>();
            return services;
        }
    }
}