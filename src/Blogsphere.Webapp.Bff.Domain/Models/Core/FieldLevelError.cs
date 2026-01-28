namespace Blogsphere.Webapp.Bff.Domain.Models.Core
{
    public class FieldLevelError
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Field { get; set; }    
    }
}