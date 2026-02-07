namespace Blogsphere.Webapp.Bff.Domain.Entities.ApiGateway
{
    public class ApiProduct
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public bool IsActive { get; set; }
        public int SubscribedApiCount { get; set; }
        public int SubscriptionCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
