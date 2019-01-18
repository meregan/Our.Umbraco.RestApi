using System.Collections.Generic;
using Microsoft.Owin.Security.Authorization;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Editors;
using Task = System.Threading.Tasks.Task;

namespace Umbraco.RestApi.Security
{
    /// <summary>
    /// An AuthZ handler to check if the user has access to the specified media item by path
    /// </summary>
    public class MediaPermissionHandler : AuthorizationHandler<MediaPermissionRequirement, ContentResourceAccess>
    {
        private readonly ServiceContext _services;

        public MediaPermissionHandler(ServiceContext services)
        {
            _services = services;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MediaPermissionRequirement requirement, ContentResourceAccess resource)
        {
            var user = context.User.GetUserFromClaims(_services.UserService);
            if (user == null)
            {
                context.Fail();
                return Task.FromResult(0);
            }

            //if there is nothing to check against then return true
            if (resource.NodeIds == null)
            {
                context.Succeed(requirement);
                return Task.FromResult(0);
            }

            foreach (var nodeId in resource.NodeIds)
            {
                IMedia media = null;
                if (nodeId != Constants.System.Root && nodeId != Constants.System.RecycleBinContent)
                {
                    media = _services.MediaService.GetById(nodeId);
                    if (media == null)
                    {
                        context.Fail();
                        return Task.FromResult(0);
                    }
                }

                var allowed = CheckPermissions(user, nodeId, media);

                if (allowed)
                    context.Succeed(requirement);
                else
                {
                    context.Fail();
                    break;
                }
            }

            return Task.FromResult(0);
        }

        private bool CheckPermissions(IUser user, int nodeId, IMedia mediaItem)
        {
            var tempStorage = new Dictionary<string, object>();
            //TODO: Using reflection, this will be public in 7.7.2
            var result = (bool)typeof(MediaController).CallStaticMethod("CheckPermissions",
                tempStorage,
                user,                
                _services.MediaService, _services.EntityService,
                nodeId, mediaItem);
            return result;
        }
    }
}