namespace Blogsphere.Webapp.Bff.Domain.Models.Dtos
{
    public class MetaDataDto
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public OperationaUserDetailsDto CreatedBy { get; set; }
        public OperationaUserDetailsDto UpdatedBy { get; set; }
    }
}
