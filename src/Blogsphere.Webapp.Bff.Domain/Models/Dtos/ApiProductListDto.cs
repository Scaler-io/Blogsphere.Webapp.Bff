using Blogsphere.Webapp.Bff.Domain.Entities.ApiGateway;

namespace Blogsphere.Webapp.Bff.Domain.Models.Dtos
{
    public class ApiProductListDto
    {
        public IReadOnlyList<ApiProduct> ApiProducts { get; set; }
        public long TotalCount { get; set; }
    }
}
