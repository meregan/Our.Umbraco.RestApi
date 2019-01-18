using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.Owin.Security.Authorization;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Editors;
using Task = System.Threading.Tasks.Task;

namespace Umbraco.RestApi.Security
{
    /// <summary>
    /// An AuthZ handler to check if the user has access to the specified content item by path and by permission
    /// </summary>
    public class ContentPermissionHandler : AuthorizationHandler<ContentPermissionRequirement, ContentResourceAccess>
    {
        private readonly ServiceContext _services;

        public ContentPermissionHandler(ServiceContext services)
        {
            _services = services;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ContentPermissionRequirement requirement, ContentResourceAccess resource)
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
                IContent content = null;
                if (nodeId != Constants.System.Root && nodeId != Constants.System.RecycleBinContent)
                {
                    content = _services.ContentService.GetById(nodeId);
                    if (content == null)
                    {
                        context.Fail();
                        return Task.FromResult(0);
                    }
                }

                var allowed = CheckPermissions(user, nodeId, 
                    //currently permissions are only one letter hence the [0] array accessor
                    requirement.Permissions.Select(x => x[0]).ToArray(), 
                    content);

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
        
        private bool CheckPermissions(IUser user, int nodeId, char[] permissionsToCheck, IContent contentItem)
        {
            var tempStorage = new Dictionary<string, object>();

            //TODO: Using reflection, We need to make this public in core!
            var result = (bool)typeof(ContentController).CallStaticMethod("CheckPermissions",
                tempStorage,
                user,
                _services.UserService, _services.ContentService, _services.EntityService,
                nodeId, permissionsToCheck, contentItem);
            return result;
        }
    }
}