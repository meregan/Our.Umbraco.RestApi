using System;
using System.Globalization;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Web.Http.Filters;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Authorization;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.RestApi.Security
{
    internal static class AuthorizationExtensions
    {
        /// <summary>
        /// Looks up the IUser instance based on the id specified in claims
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="userService"></param>
        /// <returns></returns>
        /// <remarks>
        /// TODO: Remove the need for this class - but this would require an IPermissionService that a developer could override
        /// </remarks>
        public static IUser GetUserFromClaims(this ClaimsPrincipal principal, IUserService userService)
        {
            var idClaim = principal.FindFirst(c => c.Type == ClaimTypes.NameIdentifier);
            if (idClaim == null)
            {
                return null;
            }
            var id = idClaim.Value.TryConvertTo<int>();
            if (!id)
            {
                return null;
            }
            var user = userService.GetUserById(id.Result);
            return user;
        }

        /// <summary>
        /// A policy check for all endpoints to require either the RestApiClaimType or Umbraco's SessionIdClaimType with the Umbraco issuer
        /// </summary>
        /// <param name="policy"></param>
        public static void RequireSessionIdOrRestApiClaim(this AuthorizationPolicyBuilder policy)
        {
            policy.RequireAssertion(context =>
                context.User.HasClaim(c =>
                    //to read published content the logged in user must have either of these claim types and value
                        c.Type == AuthorizationPolicies.UmbracoRestApiClaimType
                        //if we are checking the SessionIdClaimType then it should be issued from Umbraco (i.e. cookie authentication)
                        || (c.Type == Core.Constants.Security.SessionIdClaimType && c.Issuer == UmbracoBackOfficeIdentity.Issuer)));
        }

        public static CultureInfo GetUserCulture(this ClaimsPrincipal user)
        {
            var culture = user.FindFirst(ClaimTypes.Locality);
            if (culture == null)
            {
                return null;
            }
            return new CultureInfo(culture.Value);
        }

        public static string GetUserName(this ClaimsPrincipal user)
        {
            var userName = user.FindFirst(ClaimTypes.GivenName);
            return userName?.Value;
        }

        public static string GetLoginName(this ClaimsPrincipal user)
        {
            var userName = user.FindFirst(ClaimTypes.Name);
            return userName?.Value;
        }

        public static int? GetUserId(this ClaimsPrincipal user)
        {
            var id = user.FindFirst(ClaimTypes.NameIdentifier);
            if (id == null)
            {
                return null;
            }
            var intId = id.Value.TryConvertTo<int>();
            if (intId)
                return intId.Result;

            return null;
        }

        /// <summary>
        /// Returns the user's allowed sections from claims
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string[] GetAllowedSections(this ClaimsPrincipal user)
        {
            if (!user.HasClaim(c => c.Type == Constants.Security.AllowedApplicationsClaimType))
            {
                return null;
            }
            var allowedApps = user.FindAll(Constants.Security.AllowedApplicationsClaimType);
            return allowedApps?.Select(x => x.Value).ToArray();
        }

        /// <summary>
        /// Returns the users calculated start node ids from it's claims
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static int[] GetContentStartNodeIds(this ClaimsPrincipal user)
        {
            var startContentId = user.FindFirst(Constants.Security.StartContentNodeIdClaimType);
            if (startContentId == null || startContentId.Value.DetectIsJson() == false)
            {
                return null;
            }
            
            try
            {
                return JsonConvert.DeserializeObject<int[]>(startContentId.Value);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the users calculated start node ids from it's claims
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static int[] GetMediaStartNodeIds(this ClaimsPrincipal user)
        {
            var startMediaId = user.FindFirst(Constants.Security.StartMediaNodeIdClaimType);
            if (startMediaId == null || startMediaId.Value.DetectIsJson() == false)
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<int[]>(startMediaId.Value);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}