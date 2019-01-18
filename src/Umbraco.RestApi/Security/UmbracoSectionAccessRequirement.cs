using Microsoft.Owin.Security.Authorization;

namespace Umbraco.RestApi.Security
{
    public class UmbracoSectionAccessRequirement : IAuthorizationRequirement
    {
        public string Section { get; }

        public UmbracoSectionAccessRequirement(string section)
        {
            Section = section;
        }
    }
}