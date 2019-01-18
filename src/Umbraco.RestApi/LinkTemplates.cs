using Umbraco.RestApi.Routing;
using WebApi.Hal;

namespace Umbraco.RestApi
{
    internal static class LinkTemplates
    {
        public static int ApiVersion = 1;

        public static string BuildSelfUrl(string baseUrl) => $"{baseUrl}/{{id}}";
        public static string BuildParentUrl(string baseUrl) => $"{baseUrl}/{{parentId}}";
        public static string BuildMetadataUrl(string baseUrl) => $"{baseUrl}/{{id}}/meta";
        public static string BuildChildrenUrl(string baseUrl) => $"{baseUrl}/{{id}}/children{{?page,size,query}}";
        public static string BuildDescendantsUrl(string baseUrl) => $"{baseUrl}/{{id}}/descendants{{?page,size,query}}";
        public static string BuildAncestorsUrl(string baseUrl) => $"{baseUrl}/{{id}}/ancestors{{?page,size,query}}";
        public static string BuildSearchUrl(string baseUrl) => $"{baseUrl}/search{{?page,size,query}}";

        public static class Relations
        {
            public static string BaseUrl => $"~/{RouteConstants.GetRestRootPath(ApiVersion)}/{RouteConstants.RelationsSegment}";

            public static Link Root => new Link("root", BaseUrl);

            public static Link Self => new Link("relation", BuildSelfUrl(BaseUrl));

            public static Link Children => new Link("relatedChildren", $"{BaseUrl}/children/{{id}}{{?relationType}}");
            public static Link Parents => new Link("relatedParents", $"{BaseUrl}/parents/{{id}}{{?relationType}}");

            public static Link RelationType => new Link("relationType", $"{BaseUrl}/relationtype/{{alias}}");
        }

        public static class Members
        {
            public static string BaseUrl => $"~/{RouteConstants.GetRestRootPath(ApiVersion)}/{RouteConstants.MembersSegment}";

            public static Link Root => new Link("root", $"{BaseUrl}{{?page,size,query,orderBy,direction,memberTypeAlias}}");

            public static Link Self => new Link("member", BuildSelfUrl(BaseUrl));
            public static Link MetaData => new Link("meta", BuildMetadataUrl(BaseUrl));
            public static Link Search => new Link("search", BuildSearchUrl(BaseUrl));
        }

        public static class PublishedContent
        {
            public static string BaseUrl => $"~/{RouteConstants.GetRestRootPath(ApiVersion)}/{RouteConstants.ContentSegment}/{RouteConstants.PublishedSegment}";

            public static Link Root => new Link("root", BaseUrl);

            public static Link Self => new Link("content", BuildSelfUrl(BaseUrl));
            public static Link Parent => new Link("parent", BuildParentUrl(BaseUrl));
            public static Link MetaData => new Link("meta", BuildMetadataUrl(BaseUrl));
            public static Link PagedChildren => new Link("children", BuildChildrenUrl(BaseUrl));
            public static Link PagedDescendants => new Link("descendants", BuildDescendantsUrl(BaseUrl));
            public static Link PagedAncestors => new Link("ancestors", BuildAncestorsUrl(BaseUrl));
            public static Link Search => new Link("search", BuildSearchUrl(BaseUrl));

            public static Link Query => new Link("query", $"{BaseUrl}/query/{{id}}{{?page,size,query}}");
            public static Link Url => new Link("url", $"{BaseUrl}/url{{?url}}");
            public static Link Tag => new Link("tag", $"{BaseUrl}/tag/{{tag}}{{?group}}");
        }
        
        public static class Content
        {
            public static string BaseUrl => $"~/{RouteConstants.GetRestRootPath(ApiVersion)}/{RouteConstants.ContentSegment}";

            public static Link Root => new Link("root", BaseUrl);

            public static Link Self => new Link("content", BuildSelfUrl(BaseUrl));
            public static Link Parent => new Link("parent", BuildParentUrl(BaseUrl));
            public static Link MetaData => new Link("meta", BuildMetadataUrl(BaseUrl));
            public static Link PagedChildren => new Link("children", BuildChildrenUrl(BaseUrl));
            public static Link PagedDescendants => new Link("descendants", BuildDescendantsUrl(BaseUrl));
            public static Link PagedAncestors => new Link("ancestors", BuildAncestorsUrl(BaseUrl));
            public static Link Search => new Link("search", BuildSearchUrl(BaseUrl));
        }

        public static class Media
        {
            public static string BaseUrl => $"~/{RouteConstants.GetRestRootPath(ApiVersion)}/{RouteConstants.MediaSegment}";

            public static Link Root => new Link("root", BaseUrl);

            public static Link Self => new Link("content", BuildSelfUrl(BaseUrl));
            public static Link Parent => new Link("parent", BuildParentUrl(BaseUrl));
            public static Link MetaData => new Link("meta", BuildMetadataUrl(BaseUrl));
            public static Link PagedChildren => new Link("children", BuildChildrenUrl(BaseUrl));
            public static Link PagedDescendants => new Link("descendants", BuildDescendantsUrl(BaseUrl));
            public static Link PagedAncestors => new Link("ancestors", BuildAncestorsUrl(BaseUrl));
            public static Link Search => new Link("search", BuildSearchUrl(BaseUrl));

            public static Link Upload => new Link("upload", $"{BaseUrl}/{{id}}/upload{{?property,mediaType}}");
        }
    }
}
