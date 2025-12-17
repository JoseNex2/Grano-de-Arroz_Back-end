using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace GDA.Authentication
{
    public class OpaqueTokenAuthenticationHandler : AuthenticationHandler<OpaqueTokenAuthenticationSchemeOptions>
    {
        private readonly DataAccess.SupportServices.IAuthenticationService _authenticationService;
        private readonly ILogger _logger;
        public OpaqueTokenAuthenticationHandler(
            IOptionsMonitor<OpaqueTokenAuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            DataAccess.SupportServices.IAuthenticationService authenticationService
            )
            : base(options, logger, encoder)
        {
            _authenticationService = authenticationService;
            _logger = logger.CreateLogger<OpaqueTokenAuthenticationHandler>();
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("Missing Authorization Header");
            }

            var authHeader = Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader))
            {
                return AuthenticateResult.Fail("Empty Token");
            }

            string token = authHeader;
            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authHeader.Substring("Bearer ".Length).Trim();
            }

            _logger.LogInformation("Validando token opaco: {Token}", token);

            var session = await _authenticationService.ValidateAsync(token);

            if (session == null)
            {
                _logger.LogWarning("Sesión no encontrada para el token proporcionado.");
                return AuthenticateResult.Fail("Invalid Token");
            }
            if (Options.ShouldValidateLifetime)
            {
                if (session.ExpiredDate < DateTime.UtcNow)
                {
                    return AuthenticateResult.Fail("A expirado el plazo de uso");
                }
            }
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
