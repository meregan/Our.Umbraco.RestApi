using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;

namespace Umbraco.RestApi.Models.Mapping
{
    public class PublishedContentMapper : MapperConfiguration
    {
        internal const int DefaultDepth = 1;

        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {

            config.CreateMap<IPublishedContent, PublishedContentRepresentation>()
                .IgnoreHalProperties()
                .ForMember(representation => representation.CreateDate, expression => expression.MapFrom(x => x.CreateDate.ToUniversalTime())) 
                .ForMember(representation => representation.UpdateDate, expression => expression.MapFrom(x => x.UpdateDate.ToUniversalTime()))
                .ForMember(representation => representation.InternalId, expression => expression.MapFrom(x => x.Id))
                .ForMember(representation => representation.Id, expression => expression.ResolveUsing<ContentWithKeyResolver>())
                .ForMember(representation => representation.ParentId, expression => expression.ResolveUsing<ParentKeyResolver>())
                .ForMember(representation => representation.HasChildren, expression => expression.MapFrom(content => content.Children.Any()))
                .ForMember(representation => representation.Properties, expression => expression.ResolveUsing((ResolutionResult result) =>
                {
                    var content = (IPublishedContent) result.Context.SourceValue;

                    //Check the context for our special value - this allows us to only render a specific level of recursive IPublishedContent properties,
                    //since we don't want to cause it to render tons of nested picked properties.
                    var level = 0;
                    if (result.Context.Options.Items.TryGetValue("prop::level", out var levelObj))
                    {
                        level = Convert.ToInt32(levelObj);
                    }

                    var depth = 1;
                    if (result.Context.Options.Items.TryGetValue("prop::depth", out var depthObj))
                    {
                        depth = Convert.ToInt32(depthObj);
                    }

                    var d = content.Properties.ToDictionary(property => property.PropertyTypeAlias, property =>
                    {
                        
                        if (property.Value is IPublishedContent)
                        {
                            //if level is deeper than X - return null, we don't want to process endlessly
                            if (level > depth) return null;
                            //re-map but pass in a level so this recursion doesn't continue
                            return Mapper.Map<PublishedContentRepresentation>(property.Value, options =>
                            {
                                options.Items["prop::level"] = level + 1;
                                options.Items["prop::depth"] = depth;
                            });
                        }

                        if (property.Value is IEnumerable<IPublishedContent>)
                        {
                            //if level is deeper than X - return null, we don't want to process endlessly
                            if (level > depth) return null;
                            //re-map but pass in a level so this recursion doesn't continue
                            return Mapper.Map<IEnumerable<PublishedContentRepresentation>>(property.Value, options =>
                            {
                                options.Items["prop::level"] = level + 1;
                                options.Items["prop::depth"] = depth;
                            });
                        }

                        return property.Value;
                    });

                    return d;
                }));
        }

        private class ParentKeyResolver : ValueResolver<IPublishedContent, Guid>
        {
            protected override Guid ResolveCore(IPublishedContent source)
            {
                var parent = source.Parent;
                if (parent == null)
                    return Guid.Empty;

                var withKey = GetContentWithKey(parent);
                return withKey?.Key ?? Guid.Empty;
            }
        }

        private class ContentWithKeyResolver : ValueResolver<IPublishedContent, Guid>
        {
            protected override Guid ResolveCore(IPublishedContent source)
            {
                var withKey = GetContentWithKey(source);
                return withKey?.Key ?? Guid.Empty;
            }
        }

        /// <summary>
        /// Gets the published content with a guaranteed key
        /// </summary>
        /// <param name="content">The published content</param>
        /// <returns>A published content with key</returns>
        /// <remarks>
        /// Workaround for the fact that GetKey() doesn't always work - see http://issues.umbraco.org/issue/U4-10128
        /// </remarks>
        private static IPublishedContentWithKey GetContentWithKey(IPublishedContent content)
        {
            var withKey = content as IPublishedContentWithKey;

            if (withKey != null)
                return withKey;

            var wrapped = content as PublishedContentWrapped;

            if (wrapped != null)
                return GetContentWithKey(wrapped.Unwrap());

            return null;
        }
    }
}
