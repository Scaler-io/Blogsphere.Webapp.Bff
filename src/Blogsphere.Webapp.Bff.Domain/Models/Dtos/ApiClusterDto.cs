namespace Blogsphere.Webapp.Bff.Domain.Models.Dtos
{
    public class ApiClusterDto
    {
        public string Id { get; set; }
        public string ClusterId { get; set; }
        public string LoadBalancingPolicy { get; set; }
        public string HealthCheckEnabled { get; set; }
        public string HealthCheckPath { get; set; }
        public string HealthCheckInterval { get; set; }
        public string HealthCheckTimeout { get; set; }
        public bool IsActive { get; set; }
        public DestinationDto[] Destinations { get; set; }
        public ClusterRouteDto[] Routes { get; set; }
        public MetaDataDto MetaData { get; set; }
    }

    public class DestinationDto
    {
        public string Id { get; set; }
        public string DestinationId { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; }
    }

    public class ClusterRouteDto
    {
        public string Id { get; set; }
        public string RouteId { get; set; }
        public string Path { get; set; }
        public string[] Methods { get; set; }
        public bool IsActive { get; set; }
    }
}
