using System;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.RestApi.Models.Mapping
{
    internal class ParentKeyResolver : ValueResolver<IContentBase, Guid>
    {
        private readonly IEntityService _entityService;
        private readonly UmbracoObjectTypes _objectType;

        public ParentKeyResolver(IEntityService entityService, UmbracoObjectTypes objectType)
        {
            _entityService = entityService;
            _objectType = objectType;
        }

        protected override Guid ResolveCore(IContentBase source)
        {
            var attempt = _entityService.GetKeyForId(source.ParentId, _objectType);
            return attempt ? attempt.Result : Guid.Empty;
        }
    }
}