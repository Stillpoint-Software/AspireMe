using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace AspireMe.Core.Identity;

public class PrincipalProvider : IPrincipalProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PrincipalProvider( IHttpContextAccessor httpContextAccessor )
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ClaimsPrincipal User => _httpContextAccessor?.HttpContext?.User;

    public ClaimsIdentity Identity( string identityName )
    {
        return User.Identities
            .FirstOrDefault( x => identityName.Equals( x.NameClaimType, StringComparison.OrdinalIgnoreCase ) );
    }

    public string ClaimValue( string identityName, string claimType )
    {
        return Identity( identityName )?.Claims
            .FirstOrDefault( x => x.Type.Equals( claimType, StringComparison.OrdinalIgnoreCase ) )?.Value;
    }
}

public static class PrincipalProviderExtensions
{
    public static string GetEmail( this IPrincipalProvider provider )
    {
        return provider.User.Claims.FirstOrDefault( c => c.Type == AuthConstants.Claim.Email )?.Value;
    }
}
