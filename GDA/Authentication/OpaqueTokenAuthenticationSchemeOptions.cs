using Microsoft.AspNetCore.Authentication;

namespace GDA.Authentication
{
    public class OpaqueTokenAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public bool ShouldValidateLifetime { get; set; }
    }
}
