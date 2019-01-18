using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Owin;

namespace Umbraco.RestApi.Tests.TestHelpers
{
    public class AuthenticateEverythingMiddleware : AuthenticationMiddleware<AuthenticateEverythingAuthenticationOptions>
    {
        public AuthenticateEverythingMiddleware(OwinMiddleware next, IAppBuilder app, AuthenticateEverythingAuthenticationOptions options)
            : base(next, options)
        {
        }

        protected override AuthenticationHandler<AuthenticateEverythingAuthenticationOptions> CreateHandler()
        {
            return new AuthenticateEverythingHandler(Options);
        }

        public class AuthenticateEverythingHandler : AuthenticationHandler<AuthenticateEverythingAuthenticationOptions>
        {
            private readonly AuthenticateEverythingAuthenticationOptions _options;

            public AuthenticateEverythingHandler(AuthenticateEverythingAuthenticationOptions options)
            {
                _options = options;
            }

            protected override Task<AuthenticationTicket> AuthenticateCoreAsync()
            {                
                return Task.FromResult(new AuthenticationTicket(_options.UmbracoBackOfficeIdentity,
                    new AuthenticationProperties()
                    {
                        ExpiresUtc = DateTime.Now.AddDays(1)
                    }));
            }
        }
    }
}