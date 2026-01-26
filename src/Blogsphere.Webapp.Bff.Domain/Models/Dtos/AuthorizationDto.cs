namespace Blogsphere.Webapp.Bff.Domain.Models.Dtos;

public class AuthorizationDto
{
    public IReadOnlyList<string> Roles { get; set; }
    public IReadOnlyList<string> Permissions { get; set; }
    public string Token { get; set; }
}