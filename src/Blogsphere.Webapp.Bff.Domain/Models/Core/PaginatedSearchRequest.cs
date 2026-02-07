using Blogsphere.Webapp.Bff.Domain.Models.Enums;

namespace Blogsphere.Webapp.Bff.Domain.Models.Core
{
    public class PaginatedSearchRequest
    {
        public bool IsFilteredQuery { get; set; }
        public string MatchPhrase { get; set; }
        public string MatchPhraseField { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string SortField { get; set; }
        public string SortOrder { get; set; }
        public Dictionary<string, string> Filters { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string TimeField { get; set; }
        public SearchType SearchType { get; set; } = SearchType.Paginated;
    }
}
