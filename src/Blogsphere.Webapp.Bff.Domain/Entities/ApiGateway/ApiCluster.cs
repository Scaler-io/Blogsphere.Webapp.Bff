namespace Blogsphere.Webapp.Bff.Domain.Entities.ApiGateway
{
    public class ApiCluster
    {
        public string Id { get; set; }
        public string ClusterId { get; set; }
        public string LoadBalancingPolicy { get; set; }
        public string HealthCheckEnabled { get; set; }
        public string HealthCheckPath { get; set; }
        public string HealthCheckInterval { get; set; }
        public string HealthCheckTimeout { get; set; }
        public bool IsActive { get; set; }
        public Destination[] Destinations { get; set; }
        public ApiRoute[] Routes { get; set; }
        public MetaData MetaData { get; set; }
    }
}
