namespace Blogsphere.Webapp.Bff.Domain.Entities.ApiGateway
{
    public class ApiRoute
    {
        public string Id { get; set; }
        public string RouteId { get; set; }
        public string Path { get; set; }
        public string[] Methods { get; set; }
        public string IsActive { get; set; }
        public string ClusterId { get; set; }
    }
}
