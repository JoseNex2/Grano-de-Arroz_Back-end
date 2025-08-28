using GDA.Middleware;
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

            EnvironmentVariableLoader.Initialize();

            UtilitiesInversionOfControl.AddDependency(services, configuration);
            DataAccessInversionOfControl.AddDependency(services, configuration);

            services.AddAuthorization();

            services.AddControllers();

            WebApplication app = builder.Build();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}