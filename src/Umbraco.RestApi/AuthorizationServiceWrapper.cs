using System;
using Microsoft.Owin;
using Microsoft.Owin.Security.Authorization;

namespace Umbraco.RestApi
{
    /// <summary>
    /// Used as a work around to extract the <see cref="IAuthorizationService"/> from an <see cref="OwinContext"/>
    /// </summary>
    /// <remarks>
    /// see https://github.com/DavidParks8/Owin-Authorization/issues/54
    /// </remarks>
    internal class AuthorizationServiceWrapper : IDisposable
    {
        public IAuthorizationService AuthorizationService { get; }

        public AuthorizationServiceWrapper(IAuthorizationService authorizationService)
        {
            AuthorizationService = authorizationService;
        }

        public void Dispose()
        {
        }
    }
}