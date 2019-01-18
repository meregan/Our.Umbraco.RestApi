using System.Net.Http;
using System.Threading.Tasks;
using Umbraco.RestApi.Models;

namespace Umbraco.RestApi.Controllers
{
    /// <summary>
    /// This is used to ensure consistency between controllers which allows for better testing
    /// </summary>
    public interface ISearchController
    {
        Task<HttpResponseMessage> Search(PagedQuery query);
    }
}