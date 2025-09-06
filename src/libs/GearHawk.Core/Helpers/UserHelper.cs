using System.Security.Claims;

namespace GearHawk.Core.Helpers
{
    public class UserHelper
    {
        public const string sNAME_IDENTIFIER = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        public static string? UserId(ClaimsPrincipal principal)
        {
            var idClaim = principal.FindFirst(sNAME_IDENTIFIER);
            return idClaim?.Value;
        }
    }
}
