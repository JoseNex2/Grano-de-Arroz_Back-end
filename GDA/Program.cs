using GDA.Authentication;
using GDA.Middleware;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using Utilities;

namespace GDA
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog();

            var services = builder.Services;

            EnvironmentVariableLoaderHelper.Initialize();

            services.AddCors(options =>
            {
                options.AddPolicy("Desarrollo", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });

                options.AddPolicy("Produccion", policy =>
                {
                    policy.WithOrigins(Environment.GetEnvironmentVariable("URL_DOMAIN"))
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "AccessScheme";
                options.DefaultChallengeScheme = "AccessScheme";
            })
            .AddJwtBearer("AccessScheme", options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            Environment.GetEnvironmentVariable("JWT_KEY_ACCESS")
                        )
                    )
                };
            })
            .AddScheme<OpaqueTokenAuthenticationSchemeOptions, OpaqueTokenAuthenticationHandler>(
                "ExternalScheme",
                options =>
                {
                    options.ShouldValidateLifetime = true;
                });

            services.AddAuthorization();

            services.AddControllers();

            DataAccessInversionOfControl.AddDependency(services);

            var app = builder.Build();

            app.UseCors("Desarrollo");

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            Log.Information("Aplicación iniciada");

            app.Run();

            Log.CloseAndFlush();
        }
    }
}