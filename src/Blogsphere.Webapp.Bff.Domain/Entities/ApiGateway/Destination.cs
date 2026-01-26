namespace Blogsphere.Webapp.Bff.Domain.Entities.ApiGateway
{
    public class Destination
    {
        public string Id { get; set; }
        public string DestinationId { get; set; }
        public bool IsActive { get; set; }
        public string Address { get; set; }
        public string ClusterId { get; set; }
    }
}
