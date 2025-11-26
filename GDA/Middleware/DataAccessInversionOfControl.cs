using DataAccess;
using DataAccess.Generic;
using DataAccess.SupportServices;
using Entities.DataContext;
using Entities.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using System.Text;
using Utilities;

namespace GDA.Middleware
{
    public static class DataAccessInversionOfControl
    {
        public static IServiceCollection AddDependency(this IServiceCollection services, WebApplicationBuilder builder, IConfiguration configuration)
        {
            services.AddScoped<IStorageService, StorageService>();
            services.AddScoped(typeof(ISqlUnitOfWork<>), typeof(SqlUnitOfWork<>));
            services.AddScoped(typeof(ISqlGenericRepository<,>), typeof(SqlGenericRepository<,>));
            services.AddScoped(typeof(INonSqlGenericRepository<>), typeof(NonSqlGenericRepository<>));
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IBatteryService, BatteryService>();
            services.AddScoped<ICsvHelper, CsvHelper>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped(typeof(IPasswordHasher<>), typeof(PasswordHasher<>));
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<UrlEncoderHelper, UrlEncoderHelper>();
            services.AddDbContext<ServiceDbContext>(options =>
            {
                string connectionString = $"server={Environment.GetEnvironmentVariable("MYSQLDB_CONNECTION_SERVICE_HOST")};port={Environment.GetEnvironmentVariable("MYSQLDB_CONNECTION_SERVICE_PORT")};database={Environment.GetEnvironmentVariable("MYSQLDB_CONNECTION_DATABASE")};user={Environment.GetEnvironmentVariable("MYSQLDB_CONNECTION_SERVICE_USER")};password={Environment.GetEnvironmentVariable("MYSQLDB_CONNECTION_SERVICE_PASSWORD")}";
                MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder(connectionString);
                builder.ConnectionTimeout = uint.Parse(Environment.GetEnvironmentVariable("MYSQLDB_CONNECTION_TIMEOUT"));
                options.UseMySQL(
                    builder.ConnectionString,
                    mySqlOptions =>
                    {
                        mySqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                        mySqlOptions.CommandTimeout(int.Parse(Environment.GetEnvironmentVariable("MYSQLDB_COMMAND_TIMEOUT")));
                        mySqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    }
                );
            });
            services.AddScoped<IDataMongoDbContext>(sp =>
            {
                var urlEncoder = sp.GetRequiredService<UrlEncoderHelper>();

                string connectionString = $"{Environment.GetEnvironmentVariable("MONGODB_CONNECTION_PROTOCOL")}://" +
                    $"{urlEncoder.Encode(Environment.GetEnvironmentVariable("MONGODB_CONNECTION_SERVICE_USER"))}:" +
                    $"{urlEncoder.Encode(Environment.GetEnvironmentVariable("MONGODB_CONNECTION_SERVICE_PASSWORD"))}@" +
                    $"{Environment.GetEnvironmentVariable("MONGODB_CONNECTION_HOST")}:" +
                    $"{Environment.GetEnvironmentVariable("MONGODB_CONNECTION_PORT")}/" +
                    $"{Environment.GetEnvironmentVariable("MONGODB_CONNECTION_DATABASE")}" +
                    $"?authMechanism={Environment.GetEnvironmentVariable("MONGODB_CONNECTION_AUTHENTICATION")}" +
                    $"&authSource={Environment.GetEnvironmentVariable("MONGODB_CONNECTION_DATABASE_ADMIN")}";

                string databaseName = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_DATABASE")!;

                return new DataMongoDbContext(connectionString, databaseName);
            });
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
            });
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "ExternalScheme";
                options.DefaultChallengeScheme = "ExternalScheme";
            })
            .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, SecureTokenHandler>("ExternalScheme", null);
            return (services);
        }
    }
}
