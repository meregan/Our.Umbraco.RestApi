namespace Umbraco.RestApi.Models
{
    /// <summary>
    /// represents a model used for paging
    /// </summary>
    public class PagedRequest
    {
        public PagedRequest()
        {
            Page = 1;
            PageSize = 100;
        }

        /// <summary>
        /// The page number to return the results for
        /// </summary>
        /// <remarks>
        /// This is a 1 based page (not 0 based index)
        /// </remarks>
        public long Page { get; set; }

        /// <summary>
        /// The page size to return the results for
        /// </summary>
        public int PageSize { get; set; }
    }
}