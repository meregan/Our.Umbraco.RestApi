using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Umbraco.RestApi.Security
{
    public class AuthorizationPolicies
    {
        public const string UmbracoRestApiIssuer = "UmbracoRestApi";
        public const string UmbracoRestApiClaimType = "http://umbraco.org/2017/09/identity/claims/restapi";
        public const string UmbracoRestApiTokenAuthenticationType = "UmbracoRestApiTokenAuthenticationType";

        public const string PublishedContentRead = "PublishedContentRead";

        public const string MemberRead = "MemberRead";
        public const string MemberDelete = "MemberDelete";
        public const string MemberUpdate = "MemberUpdate";
        public const string MemberCreate = "MemberCreate";

        public const string MediaRead = "MediaRead";
        public const string MediaCreate = "MediaCreate";
        public const string MediaUpdate = "MediaUpdate";
        public const string MediaDelete = "MediaDelete";

        public const string ContentRead = "ContentRead";
        public const string ContentCreate = "ContentCreate";
        public const string ContentUpdate = "ContentUpdate";
        public const string ContentDelete = "ContentDelete";

        public const string DefaultRestApi = "DefaultRestApi";
    }
}
