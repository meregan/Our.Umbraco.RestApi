using System;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Services;

namespace Umbraco.RestApi.Models.Mapping
{
    public class RelationModelMapper : MapperConfiguration
    {
        
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<IRelation, RelationRepresentation>()
                .IgnoreHalProperties()
                .ForMember(representation => representation.Id, expression => expression.MapFrom(x => x.Id))
                .ForMember(
                    representation => representation.ChildId,
                    expression => expression.ResolveUsing(
                        new RelationKeyResolver(RelationDirection.Child, applicationContext.Services.EntityService)))
                .ForMember(
                    representation => representation.ParentId,
                    expression => expression.ResolveUsing(
                        new RelationKeyResolver(RelationDirection.Parent, applicationContext.Services.EntityService)))
                .ForMember(representation => representation.CreateDate, expression => expression.MapFrom(x => x.CreateDate.ToUniversalTime()))
                .ForMember(representation => representation.UpdateDate, expression => expression.MapFrom(x => x.UpdateDate.ToUniversalTime()))
                .ForMember(representation => representation.RelationTypeAlias, expression => expression.MapFrom(member => member.RelationType.Alias));
            
            config.CreateMap<IRelationType, RelationTypeRepresentation>()
                .IgnoreHalProperties()
                .ForMember(rep => rep.ParentEntityType, ex => ex.ResolveUsing(content => ConvertGuidToPublishedType(content.ParentObjectType)))
                .ForMember(rep => rep.ChildEntityType, ex => ex.ResolveUsing(content => ConvertGuidToPublishedType(content.ChildObjectType)));

            config.CreateMap<RelationRepresentation, IRelation>()
                .ConstructUsing((RelationRepresentation source) =>
                {
                    var intParentId = applicationContext.Services.EntityService.GetIdForKey(source.ParentId, UmbracoObjectTypes.Unknown);
                    var intChildId = applicationContext.Services.EntityService.GetIdForKey(source.ChildId, UmbracoObjectTypes.Unknown);
                    return new Relation(intParentId.Result, intChildId.Result, applicationContext.Services.RelationService.GetRelationTypeByAlias(source.RelationTypeAlias));
                })
                .ForMember(dto => dto.DeletedDate, expression => expression.Ignore())
                .ForMember(dto => dto.UpdateDate, expression => expression.Ignore())
                .ForMember(dto => dto.Key, expression => expression.Ignore())
                .ForMember(dto => dto.ParentId, expression => expression.Ignore())  //ignored because this is set in the ctor
                .ForMember(dto => dto.ChildId, expression => expression.Ignore())   //ignored because this is set in the ctor
                .ForMember(dto => dto.RelationType, expression => expression.MapFrom(x => applicationContext.Services.RelationService.GetRelationTypeByAlias(x.RelationTypeAlias)))
                .ForMember(dest => dest.Id, expression => expression.Condition(representation => (representation.Id > 0)));
        }

        private enum RelationDirection
        {
            Child, Parent
        }
        
        private class RelationKeyResolver : ValueResolver<IRelation, Guid>
        {
            private readonly RelationDirection _dir;
            private readonly IEntityService _entityService;

            public RelationKeyResolver(RelationDirection dir, IEntityService entityService)
            {
                _dir = dir;
                _entityService = entityService;
            }

            protected override Guid ResolveCore(IRelation source)
            {
                var attempt = _entityService.GetKeyForId(_dir == RelationDirection.Parent ? source.ParentId : source.ChildId, UmbracoObjectTypes.Unknown);
                return attempt ? attempt.Result : Guid.Empty;
            }
        }

        private static PublishedItemType ConvertGuidToPublishedType(Guid guid)
        {
            if (guid == Constants.ObjectTypes.DocumentGuid)
                return PublishedItemType.Content;

            if (guid == Constants.ObjectTypes.MediaGuid)
                return PublishedItemType.Media;

            if (guid == Constants.ObjectTypes.MemberGuid)
                return PublishedItemType.Member;
            
            //default return value
            return PublishedItemType.Content;
        } 
        
    }
}
