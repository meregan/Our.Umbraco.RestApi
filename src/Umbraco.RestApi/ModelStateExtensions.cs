using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Http.ModelBinding;

namespace Umbraco.RestApi
{
    internal static class ModelStateExtensions
    {

        /// <summary>
        /// Adds the error to model state correctly for a property so we can use it on the client side.
        /// </summary>
        /// <param name="modelState"></param>
        /// <param name="result"></param>
        /// <param name="propertyAlias"></param>
        internal static void AddPropertyError(this ModelStateDictionary modelState, ValidationResult result, string propertyAlias)
        {
            //if there are no member names supplied then we assume that the validation message is for the overall property
            // not a sub field on the property editor
            if (!result.MemberNames.Any())
            {
                //add a model state error for the entire property
                modelState.AddModelError(string.Format("{0}.{1}.{2}", "content","properties", propertyAlias), result.ErrorMessage);
            }
            else
            {
                //there's assigned field names so we'll combine the field name with the property name
                // so that we can try to match it up to a real sub field of this editor
                foreach (var field in result.MemberNames)
                {
                    modelState.AddModelError(string.Format("{0}.{1}.{2}.{3}", "content","properties", propertyAlias, field), result.ErrorMessage);
                }
            }
        }

      
    }
}