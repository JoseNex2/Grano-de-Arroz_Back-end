using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace GDA.Authentication
{
    public class OpaqueTokenAuthenticationHandler : AuthenticationHandler<OpaqueTokenAuthenticationSchemeOptions>
    {
        private readonly DataAccess.SupportServices.IAuthenticationService _authenticationService;
        public OpaqueTokenAuthenticationHandler(
            IOptionsMonitor<OpaqueTokenAuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            DataAccess.SupportServices.IAuthenticationService authenticationService)
            : base(options, logger, encoder)
        {
            _authenticationService = authenticationService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("Unauthorized");
            }

            var token = Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(token))
            {
                return AuthenticateResult.Fail("Unauthorized");
            }

            Logger.LogInformation("ExternalScheme handler ejecutado. Token recibido: {token}", token);

            var session = await _authenticationService.ValidateAsync(token);

            if (session == null)
            {
                return AuthenticateResult.Fail("Unauthorized");
            }
            Logger.LogInformation("Token encontrado en base de datos: {session}", session);
            if (Options.ShouldValidateLifetime)
            {
                if (session.ExpiredDate < DateTime.UtcNow)
                {
                    return AuthenticateResult.Fail("Unauthorized");
                }
            }
            Logger.LogInformation("La fecha esta dentro del rango");
            var user = session.User;

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
            };

            var identity = new ClaimsIdentity(claims, authenticationType: Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, authenticationScheme: Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}
