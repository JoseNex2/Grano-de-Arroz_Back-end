using Entities.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

public class SecureTokenHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly DataAccess.SupportServices.IAuthenticationService _authenticationService;

    public SecureTokenHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        DataAccess.SupportServices.IAuthenticationService authenticationService)
        : base(options, logger, encoder)
    {
        _authenticationService = authenticationService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Logger.LogInformation("Entró al SecureTokenHandler para autenticación.");

        if (!Request.Headers.ContainsKey("Authorization"))
        {
            Logger.LogWarning("Falta el header Authorization.");
            return AuthenticateResult.Fail("Missing Authorization Header");
        }

        string header = Request.Headers["Authorization"].ToString();
        Logger.LogInformation("Authorization header recibido: '{Header}'", header);

        string token = header.Trim();
        Logger.LogInformation("Token luego de Trim: '{Token}'", token);

        SecureRandomToken result = await _authenticationService.ValidateAsync(token);

        if (result == null)
        {
            Logger.LogWarning("ValidateAsync devolvió NULL -> token inválido o expirado.");
            return AuthenticateResult.Fail("Invalid or expired token");
        }

        Logger.LogInformation("Token válido. Usuario asociado: {UserId}", result.User?.Id);

        User user = result.User!;

        List<Claim> claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim(ClaimTypes.Email, user.Email)
    };

        if (!string.IsNullOrEmpty(user.Role?.Name))
        {
            Logger.LogInformation("Rol del usuario: {Role}", user.Role.Name);
            claims.Add(new Claim(ClaimTypes.Role, user.Role.Name));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);

        Logger.LogInformation("Autenticación exitosa con el esquema: {Scheme}", Scheme.Name);

        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }


}
