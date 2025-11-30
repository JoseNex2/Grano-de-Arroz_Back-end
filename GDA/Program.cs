using GDA.Extension;
using GDA.Middleware;
using Serilog;
using Utilities;

namespace GDA
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            ConfigurationManager configuration = builder.Configuration;

            IServiceCollection services = builder.Services;

            EnvironmentVariableLoaderHelper.Initialize();

            UtilitiesInversionOfControl.AddDependency(services, builder, configuration);
            DataAccessInversionOfControl.AddDependency(services, builder, configuration);

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

            services.AddAuthorization();

            services.AddControllers();

            WebApplication app = builder.Build();

            app.UseCors("Desarrollo");
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseUserLogging();
            app.MapControllers();
            Log.Information("Aplicación iniciada");
            app.Run();
            Log.CloseAndFlush();
        }
    }
}