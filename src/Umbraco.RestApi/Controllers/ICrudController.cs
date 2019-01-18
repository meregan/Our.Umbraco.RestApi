using System.Net.Http;
using System.Threading.Tasks;
using Umbraco.RestApi.Models;
using WebApi.Hal;

namespace Umbraco.RestApi.Controllers
{
    /// <summary>
    /// This is used to ensure consistency between controllers which allows for better testing
    /// </summary>
    public interface ICrudController<in TKey, in TRepresentation> 
        where TRepresentation : Representation
    {
        Task<HttpResponseMessage> Get(TKey id);
        Task<HttpResponseMessage> Post(TRepresentation content);
        Task<HttpResponseMessage> Put(TKey id, TRepresentation content);
        Task<HttpResponseMessage> Delete(TKey id);
    }
}