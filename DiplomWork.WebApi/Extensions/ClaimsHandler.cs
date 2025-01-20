using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DiplomWork.WebApi.Extensions
{
    public static class ClaimsHandler
    {
        public static Guid? GetClaimsUserId(this ControllerBase cbase, ClaimsPrincipal userClaims)
        {
            var identity = userClaims.Identity as ClaimsIdentity;
            if (userClaims.Identity != null)
            {
                var userId = identity.FindFirst("UserId").Value;
                return Guid.Parse(userId);
            }

            return null;
        }
    }
}
