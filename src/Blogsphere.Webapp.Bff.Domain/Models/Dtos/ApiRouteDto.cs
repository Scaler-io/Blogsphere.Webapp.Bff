namespace Blogsphere.Webapp.Bff.Domain.Models.Dtos
{
    public class ApiRouteDto
    {
        public string Id { get; set; }
        public string RouteId { get; set; }
        public string Path { get; set; }
        public List<string> Methods { get; set; }
        public string RateLimiterPolicy { get; set; }
        public MetaDataDto Metadata { get; set; }
        public bool IsActive { get; set; }
        public string ClusterId { get; set; }
        public List<ApiRouteHeaderDto> Headers { get; set; }
        public List<ApiRouteTransformDto> Transforms { get; set; }
    }

    public class ApiRouteTransformDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> Values { get; set; }
        public string Mode { get; set; }
        public bool IsActive { get; set; }
    }

    public class ApiRouteHeaderDto
    {
        public string Id { get; set; }
        public string PathPattern { get; set; }
        public bool IsActive { get; set; }
    }
}
