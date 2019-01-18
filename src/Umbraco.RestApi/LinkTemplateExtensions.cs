using System;
using WebApi.Hal;

namespace Umbraco.RestApi
{
    public static class LinkTemplateExtensions
    {
        public static Link CreateLinkTemplate(this Link link, int id)
        {
            link.Href = link.Href.Replace("{id}", id.ToString());
            return link;
        }

        public static Link CreateLinkTemplate(this Link link, Guid id)
        {
            link.Href = link.Href.Replace("{id}", id.ToString());
            return link;
        }
    }
}
