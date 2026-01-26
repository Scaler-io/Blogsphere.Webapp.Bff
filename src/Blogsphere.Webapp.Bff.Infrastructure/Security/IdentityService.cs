using System.Security.Claims;
using Blogsphere.Webapp.Bff.Application.Contracts.Security;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos;
using IdentityModel;
using Microsoft.AspNetCore.Http;

namespace Blogsphere.Webapp.Bff.Infrastructure.Security
{

    public class IdentityService(IHttpContextAccessor httpContextAccessor) : IdentityBase, IIdentityService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public const string IdClaim = ClaimTypes.NameIdentifier;
        public const string RoleClaim = ClaimTypes.Role;
        public const string FirstNameClaim = ClaimTypes.GivenName;
        public const string LastNameClaim = ClaimTypes.Surname;
        public const string UsernameClaim = JwtClaimTypes.Name;
        public const string EmailClaim = ClaimTypes.Email;
        public const string PermissionClaim = "permissions";

        public UserDto PrepareUser()
        {
            var claims = _httpContextAccessor.HttpContext?.User?.Claims;
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"]; // remove Bearer prefix
            if (!string.IsNullOrEmpty(token))
            {
                token = token.ToString().Replace("Bearer ", "");
            }

            if (claims == null || !claims.Any())
            {
                return null;
            }

            var permissionsString = claims.FirstOrDefault(c => c.Type == PermissionClaim)?.Value;
            var id = claims.FirstOrDefault(c => c.Type == IdClaim)?.Value;
            var firstName = claims.FirstOrDefault(c => c.Type == FirstNameClaim)?.Value;
            var lastName = claims.FirstOrDefault(c => c.Type == LastNameClaim)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == UsernameClaim)?.Value;
            var email = claims.FirstOrDefault(c => c.Type == EmailClaim)?.Value;
            var roleString = claims.FirstOrDefault(c => c.Type == RoleClaim)?.Value;

            // Parse permissions - handle wildcard, JSON array, plain string, or null
            var permissions = ParseClaimValue(permissionsString);

            // Parse roles - handle wildcard, JSON array, plain string, or null
            var roles = ParseClaimValue(roleString);

            return new UserDto()
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                UserName = name,
                Email = email,
                Authorization = new AuthorizationDto()
                {
                    Roles = roles,
                    Permissions = permissions,
                    Token = token
                }
            };
        }
    }
}