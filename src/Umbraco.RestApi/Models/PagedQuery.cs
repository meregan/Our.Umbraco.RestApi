namespace Umbraco.RestApi.Models
{
    /// <summary>
    /// A query structure that gets parsed for incoming REST API calls that support query structures.
    /// </summary>
    /// <remarks>
    /// Querying needs to be very flexible, therefore the QueryStructure allows for varying types of queries to be executed
    /// and since we don't necessarily want to parse a single query type into other native query types, we'll allow for 
    /// multiple query expressions.
    /// 
    /// For example, a developer can pass in a RAW lucene query structure, or
    ///     TODO: Potentially support JsonPath which would be nice: http://goessner.net/articles/JsonPath/ - hopefully could find a .net implementation -> Newtonsoft.net supports JSONPath!
    ///     TODO: Potentially support a robust query structure like jsData: http://www.js-data.io/v1.6.0/docs/query-syntax
    ///     TODO: Potentially support a robust query structure like breeze: http://www.getbreezenow.com/documentation/query-using-json or even the standard OData query structure
    ///             though I think that both of those will require an IQueryable implementation which has been started in the Umbraco LinqPad project
    ///     Some other query structures that might be of interest: http://orangevolt.blogspot.com.au/2012/12/8-ways-to-query-json-structures.html
    /// </remarks>
    public class PagedQuery : PagedRequest
    {
        /// <summary>
        /// The query to lookup results for
        /// </summary>
        /// <remarks>
        /// Depending on how this structure is used could mean this query is of different formats
        /// </remarks>
        public string Query { get; set; }
    }
}