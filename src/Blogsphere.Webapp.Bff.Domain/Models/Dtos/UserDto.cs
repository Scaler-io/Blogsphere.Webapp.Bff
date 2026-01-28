namespace Blogsphere.Webapp.Bff.Domain.Models.Dtos
{
    public class UserDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public AuthorizationDto Authorization { get; set; }
    }
}