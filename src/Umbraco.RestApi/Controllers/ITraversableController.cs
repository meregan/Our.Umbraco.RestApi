using System.Net.Http;
using System.Threading.Tasks;
using Umbraco.RestApi.Models;

namespace Umbraco.RestApi.Controllers
{
    /// <summary>
    /// This is used to ensure consistency between controllers which allows for better testing
    /// </summary>
    public interface ITraversableController<in TKey, in TRepresentation> : ISearchController, ICrudController<TKey, TRepresentation>, IRootController, IMetadataController
        where TRepresentation : ContentRepresentationBase
    {
        Task<HttpResponseMessage> GetChildren(TKey id, PagedQuery query);

        Task<HttpResponseMessage> GetDescendants(TKey id, PagedQuery query);

        Task<HttpResponseMessage> GetAncestors(TKey id, PagedRequest query);
    }
}