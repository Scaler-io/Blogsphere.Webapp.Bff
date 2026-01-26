using Blogsphere.Webapp.Bff.Domain.Models.Dtos;

namespace Blogsphere.Webapp.Bff.Domain.Models.Core;

public class RequestInformation
{
    public string CorreationId { get; set; }
    public UserDto CurrentUser { get; set; }
}