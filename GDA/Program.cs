using GDA.Middleware;
using Serilog;
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

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            ConfigurationManager configuration = builder.Configuration;

            IServiceCollection services = builder.Services;

            EnvironmentVariableLoaderHelper.Initialize();
            DataAccessInversionOfControl.AddDependency(services);

            services.AddCors(options =>
            {
                options.AddPolicy("Desarrollo", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
                options.AddPolicy("Produccion", builder =>
                {
                    builder.WithOrigins(Environment.GetEnvironmentVariable("URL_DOMAIN"))
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });

            builder.Host.UseSerilog();

            services.AddAuthorization();

            services.AddControllers();

            WebApplication app = builder.Build();

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