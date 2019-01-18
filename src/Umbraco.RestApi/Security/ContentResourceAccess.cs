using Umbraco.Core.Models;

namespace Umbraco.RestApi.Security
{
    /// <summary>
    /// A resource object that is passed to the <see cref="ContentPermissionHandler"/>
    /// </summary>
    public class ContentResourceAccess
    {
        public int[] NodeIds { get; }

        private ContentResourceAccess()
        {
        }

        private static readonly ContentResourceAccess EmptyInstance = new ContentResourceAccess();
        public static ContentResourceAccess Empty()
        {
            return EmptyInstance;
        }

        public ContentResourceAccess(int[] nodeIds)
        {
            NodeIds = nodeIds;
        }
        
        public ContentResourceAccess(int nodeId)
        {
            NodeIds = new []{nodeId};
        }        
    }
}