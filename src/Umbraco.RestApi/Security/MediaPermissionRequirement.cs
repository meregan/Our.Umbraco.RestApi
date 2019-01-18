using Microsoft.Owin.Security.Authorization;

namespace Umbraco.RestApi.Security
{
    /// <summary>
    /// An AuthZ requirement for validating that the user has permission for a media item
    /// </summary>
    public class MediaPermissionRequirement : IAuthorizationRequirement
    {
    }
}