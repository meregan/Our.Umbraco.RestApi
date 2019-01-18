using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;

namespace Umbraco.RestApi.Security
{

    /// <summary>
    /// This inspects the result of the action that returns a collection of content and removes 
    /// any item that the current user doesn't have access to
    /// </summary>
    /// <remarks>
    /// This is very similar to Umbraco Core's FilterAllowedOutgoingContentAttribute but that is internal and also requires the use of 
    /// singletons which we don't want so there is some logic replicated here
    /// </remarks>
    internal class FilterAllowedOutgoingContent
    {
        private readonly IUserService _userService;
        private readonly string _permissionToCheck;

        public FilterAllowedOutgoingContent(IUserService userService, string permissionToCheck)
        {
            _userService = userService;
            _permissionToCheck = permissionToCheck;
        }
        
        internal void FilterBasedOnPermissions(IList items, IUser user)
        {
            var length = items.Count;

            if (length > 0)
            {
                var ids = new List<int>();
                for (var i = 0; i < length; i++)
                {
                    ids.Add(((dynamic)items[i]).InternalId);
                }
                //get all the permissions for these nodes in one call
                var userPermissions = _userService.GetPermissions(user, ids.ToArray());

                //if these are null it means were testing
                if (userPermissions == null)
                    return;

                var permissions = userPermissions.ToArray();

                var toRemove = new List<dynamic>();
                foreach (dynamic item in items)
                {
                    var nodePermission = permissions.Where(x => x.EntityId == Convert.ToInt32(item.InternalId)).ToArray();
                    //if there are no permissions for this id then we need to check what the user's default
                    // permissions are.
                    if (nodePermission.Length == 0)
                    {
                        //var defaultP = user.DefaultPermissions

                        toRemove.Add(item);
                    }
                    else
                    {
                        foreach (var n in nodePermission)
                        {
                            //if the permission being checked doesn't exist then remove the item
                            if (n.AssignedPermissions.Contains(_permissionToCheck.ToString(CultureInfo.InvariantCulture)) == false)
                            {
                                toRemove.Add(item);
                            }
                        }
                    }
                }
                foreach (var item in toRemove)
                {
                    items.Remove(item);
                }
            }
        }
    }
}