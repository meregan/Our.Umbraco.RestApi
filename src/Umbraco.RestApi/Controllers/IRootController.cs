using System.Net.Http;
using System.Threading.Tasks;

namespace Umbraco.RestApi.Controllers
{
    /// <summary>
    /// This is used to ensure consistency between controllers which allows for better testing
    /// </summary>
    public interface IRootController
    {
        /// <summary>
        /// Returns the items at the root
        /// </summary>
        /// <returns></returns>
        Task<HttpResponseMessage> Get();
    }
}