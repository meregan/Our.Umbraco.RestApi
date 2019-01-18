using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;

namespace Umbraco.RestApi.Models.Mapping
{
    public class MediaModelMapper : MapperConfiguration
    {

        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<IMedia, MediaRepresentation>()
                .IgnoreHalProperties()
                .ForMember(representation => representation.InternalId, expression => expression.MapFrom(x => x.Id))
                .ForMember(representation => representation.Id, expression => expression.MapFrom(x => x.Key))
                .ForMember(
                    representation => representation.ParentId,
                    expression => expression.ResolveUsing(
                        new ParentKeyResolver(applicationContext.Services.EntityService, UmbracoObjectTypes.Media)))
                .ForMember(representation => representation.CreateDate, expression => expression.MapFrom(x => x.CreateDate.ToUniversalTime()))
                .ForMember(representation => representation.UpdateDate, expression => expression.MapFrom(x => x.UpdateDate.ToUniversalTime()))
                .ForMember(representation => representation.HasChildren, expression => expression.MapFrom(content =>
                    applicationContext.Services.MediaService.HasChildren(content.Id)))
                .ForMember(representation => representation.Properties, expression => expression.ResolveUsing<ContentPropertiesResolver>());
            
            config.CreateMap<IMedia, ContentCreationTemplate>()
                .IgnoreAllUnmapped()
                .ForMember(representation => representation.Properties, expression => expression.ResolveUsing(content =>
                {
                    return content.PropertyTypes.ToDictionary<PropertyType, string, object>(propertyType => propertyType.Alias, propertyType => "");
                }));

            config.CreateMap<IMedia, IDictionary<string, ContentPropertyInfo>>()
                .ConstructUsing(content =>
                {
                    var result = new Dictionary<string, ContentPropertyInfo>();
                    foreach (var propertyType in content.PropertyTypes)
                    {
                        result[propertyType.Alias] = new ContentPropertyInfo
                        {
                            Label = propertyType.Name,
                            ValidationRegexExp = propertyType.ValidationRegExp,
                            ValidationRequired = propertyType.Mandatory
                        };
                    }
                    return result;
                });

            config.CreateMap<MediaRepresentation, IMedia>()
                .IgnoreAllUnmapped()
                .ForMember(content => content.Name, expression => expression.MapFrom(representation => representation.Name))
                //TODO: This could be used to 'Move' an item but we'd have to deal with that, not sure we should deal with that in a mapping
                //.ForMember(content => content.ParentId, expression => expression.MapFrom(representation => representation.ParentId))
                //TODO: This could be used to 'Sort' an item but we'd have to deal with that, not sure we should deal with that in a mapping
                //.ForMember(content => content.SortOrder, expression => expression.MapFrom(representation => representation.SortOrder))
                .AfterMap((representation, content) =>
                {
                    if (representation.Properties != null)
                    {
                        //TODO: Map template;
                        foreach (var propertyRepresentation in representation.Properties)
                        {
                            var found = content.HasProperty(propertyRepresentation.Key) ? content.Properties[propertyRepresentation.Key] : null;
                            if (found != null)
                            {
                                found.Value = propertyRepresentation.Value;
                            }
                        }
                    }

                });
        }
    }
}
