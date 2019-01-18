using System;
using Umbraco.Core.Models;

namespace Umbraco.RestApi.Controllers
{
    public interface IPublishedContentRequestFactory
    {
        void Create(IPublishedContent content, Uri requestUri);
    }
}