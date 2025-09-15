using Serilog.Context;
using System.Security.Claims;

namespace GDA.Extensions
{
    public static class AppBuilderExtensions
    {
        public static IApplicationBuilder UseUserLogging(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                var userId = context.User?.FindFirst("sub")?.Value ??
                             context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                             context.User?.Identity?.Name;

                var userName = context.User?.FindFirst(ClaimTypes.Name)?.Value ??
                               context.User?.Identity?.Name;

                using (LogContext.PushProperty("UserId", userId))
                using (LogContext.PushProperty("UserName", userName))
                using (LogContext.PushProperty("RequestPath", context.Request.Path))
                using (LogContext.PushProperty("ClientIp", context.Connection.RemoteIpAddress?.ToString()))
                using (LogContext.PushProperty("RequestId", context.Request.Headers["X-Request-ID"].FirstOrDefault()))
                {
                    await next();
                }
            });
        }
    }
}
