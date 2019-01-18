using System;
using Microsoft.Owin.Security;
using Umbraco.Core.Security;

namespace Umbraco.RestApi.Tests.TestHelpers
{
    public class AuthenticateEverythingAuthenticationOptions : AuthenticationOptions
    {
        public AuthenticateEverythingAuthenticationOptions()
            : base("AuthenticateEverything")
        {
            AuthenticationMode = AuthenticationMode.Active;
            var identity = new UmbracoBackOfficeIdentity(
                new UserData(Guid.NewGuid().ToString())
                {
                    Id = 0,
                    Roles = new[] { "admin" },
                    AllowedApplications = new[] { "content", "media", "members" },
                    Culture = "en-US",
                    RealName = "Admin",
                    StartContentNodes = new[] { -1 },
                    StartMediaNodes = new[] { -1 },
                    Username = "admin",
                    SecurityStamp = Guid.NewGuid().ToString(),
                    SessionId = Guid.NewGuid().ToString()
                });
            UmbracoBackOfficeIdentity = identity;
        }

        public AuthenticateEverythingAuthenticationOptions(UmbracoBackOfficeIdentity identity)
            : base("AuthenticateEverything")
        {
            AuthenticationMode = AuthenticationMode.Active;
            UmbracoBackOfficeIdentity = identity;
        }

        public UmbracoBackOfficeIdentity UmbracoBackOfficeIdentity { get; private set; }
    }
}