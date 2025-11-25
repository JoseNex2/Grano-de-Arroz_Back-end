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
        if (!Request.Headers.ContainsKey("Authorization"))
            return AuthenticateResult.Fail("Missing Authorization Header");

        string header = Request.Headers["Authorization"].ToString();

        if (!header.StartsWith("Secure ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.Fail("Invalid scheme");

        string token = header.Substring("Secure ".Length).Trim();

        var result = await _authenticationService.ValidateAsync(token);

        if (result == null)
            return AuthenticateResult.Fail("Invalid or expired token");

        var user = result.User!;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name)
        };

        // Rol
        if (!string.IsNullOrEmpty(user.Role.Name))
            claims.Add(new Claim(ClaimTypes.Role, user.Role.Name));

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
