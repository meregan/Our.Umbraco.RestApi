using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.ModelBinding;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.RestApi.Models;

namespace Umbraco.RestApi.Controllers
{
    /// <summary>
    /// A validation helper class used to validate content properties
    /// </summary>
    /// <typeparam name="TPersisted"></typeparam>    
    internal class ContentPropertyValidator<TPersisted>
        where TPersisted : class, IContentBase
    {
        private readonly ModelStateDictionary _modelState;
        private readonly IDataTypeService _dataTypeService;

        public ContentPropertyValidator(ModelStateDictionary modelState, IDataTypeService dataTypeService)
        {
            if (modelState == null) throw new ArgumentNullException("modelState");
            if (dataTypeService == null) throw new ArgumentNullException("dataTypeService");
            _modelState = modelState;
            _dataTypeService = dataTypeService;
        }

        public void ValidateItem(ContentRepresentationBase postedItem, TPersisted content)
        {
            if (postedItem == null) throw new ArgumentNullException("postedItem");
            if (content == null) throw new ArgumentNullException("content");

            //now do each validation step
            if (ValidateProperties(postedItem, content) == false) return;
            if (ValidatePropertyData(postedItem.Properties, content) == false) return;
        }
        
        /// <summary>
        /// Ensure all of properties exist
        /// </summary>
        /// <param name="postedItem"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        protected bool ValidateProperties(ContentRepresentationBase postedItem, TPersisted content)
        {
            var hasErrors = false;

            if (postedItem.Properties != null)
            {
                foreach (var p in postedItem.Properties)
                {
                    if (!content.HasProperty(p.Key))
                    {
                        //TODO: Do we return errors here ? If someone deletes a property whilst their editing then should we just
                        //save the property data that remains? Or inform them they need to reload... not sure. This problem exists currently too i think.

                        var message = string.Format("property with alias: {0} was not found", p.Key);
                        _modelState.AddModelError(string.Format("content.properties.{0}", p.Key), message);

                        hasErrors = true;
                    }
                }
            }
            return !hasErrors;
        }


        /// <summary>
        /// Validates the data for each property
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// All property data validation goes into the modelstate with a prefix of "Properties"
        /// </remarks>
        protected bool ValidatePropertyData(IDictionary<string, object> postedProperties, TPersisted content)
        {
            var hasError = false;
            if (postedProperties != null)
            {
                foreach (var p in postedProperties)
                {

                    var propertyType = content.PropertyTypes.Single(x => x.Alias == p.Key);

                    var editor = PropertyEditorResolver.Current.GetByAlias(propertyType.PropertyEditorAlias);

                    if (editor == null)
                    {
                        var message = string.Format("The property editor with alias: {0} was not found for property with id {1}", propertyType.PropertyEditorAlias, content.Properties[p.Key].Id);
                        LogHelper.Warn<ContentPropertyValidator<TPersisted>>(message);
                        continue;
                    }

                    //get the posted value for this property
                    var postedValue = p.Value;

                    //get the pre-values for this property
                    var preValues = _dataTypeService.GetPreValuesCollectionByDataTypeId(propertyType.DataTypeDefinitionId);

                    foreach (var result in editor.ValueEditor.Validators.SelectMany(v => v.Validate(postedValue, preValues, editor)))
                    {
                        _modelState.AddPropertyError(result, p.Key);
                    }

                    //Now we need to validate the property based on the PropertyType validation (i.e. regex and required)
                    // NOTE: These will become legacy once we have pre-value overrides.
                    if (propertyType.Mandatory)
                    {
                        foreach (var result in editor.ValueEditor.RequiredValidator.Validate(postedValue, "", preValues, editor))
                        {
                            hasError = true;
                            _modelState.AddPropertyError(result, p.Key);
                        }
                    }

                    if (propertyType.ValidationRegExp.IsNullOrWhiteSpace() == false)
                    {
                        foreach (var result in editor.ValueEditor.RegexValidator.Validate(postedValue, propertyType.ValidationRegExp, preValues, editor))
                        {
                            hasError = true;
                            _modelState.AddPropertyError(result, p.Key);
                        }
                    }
                }
            }

            return !hasError;
        }


    }
}