using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Microsoft.Owin.Security.Authorization;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.RestApi.Models;
using Umbraco.Web;
using Umbraco.Web.WebApi;
using WebApi.Hal;
using Microsoft.AspNet.Identity.Owin;
using WebApi.Hal.JsonConverters;

namespace Umbraco.RestApi.Controllers
{
    [DynamicCors]    
    [IsBackOffice]
    [HalFormatterConfiguration]
    public abstract class UmbracoHalController : UmbracoApiControllerBase
    {
        private IAuthorizationService _authorizationService;

        protected UmbracoHalController()
        {
            
        }

        protected UmbracoHalController(
            UmbracoContext umbracoContext,
            UmbracoHelper umbracoHelper)
            : base(umbracoContext, umbracoHelper)
        {
        }

        /// <summary>
        /// Expose the <see cref="IAuthorizationService"/> from the OwinContext in order to authorize 'Documents'
        /// </summary>
        /// <remarks>
        /// This is required for any authorization that requires a resource such as a Context (i.e. Content item, etc...)
        /// Authorization via Attributes only provides so much information, if more granular authorization is required then it needs to be done
        /// in inline code using the <see cref="IAuthorizationService"/>
        /// </remarks>
        protected IAuthorizationService AuthorizationService
        {
            get
            {
                if (_authorizationService == null)
                {
                    var authService = Request.GetOwinContext().Get<AuthorizationServiceWrapper>();
                    if (authService == null)
                    {
                        throw new InvalidOperationException("No " + typeof(IAuthorizationService) + " was found in the OwinContext, ensure ConfigureUmbracoRestApiAuthorizationPolicies has been executed");
                    }
                    else
                        _authorizationService = authService.AuthorizationService;
                }
                return _authorizationService;
            }   
        }

        /// <summary>
        /// Exposes the <see cref="ClaimsPrincipal"/> for the request
        /// </summary>
        /// <remarks>
        /// This is the current user assigned to the request
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// An exception is thrown if it is not an instance of <see cref="ClaimsPrincipal"/>
        /// </exception>
        protected ClaimsPrincipal ClaimsPrincipal
        {
            get
            {
                if (!(User is ClaimsPrincipal claimsPrincipal))
                    throw new InvalidOperationException("The current principal must be of type " + typeof(ClaimsPrincipal));
                return claimsPrincipal;
            }
        }

        /// <summary>
        /// Returns the current API version from the request
        /// </summary>
        protected int CurrentVersionRequest => int.Parse(Regex.Match(Request.RequestUri.AbsolutePath, "/v(\\d+)/", RegexOptions.Compiled).Groups[1].Value);
        
        /// <summary>
        /// Used to throw validation exceptions
        /// </summary>
        /// <param name="modelState"></param>
        /// <param name="content"></param>
        /// <param name="linkTemplate"></param>
        /// <param name="message"></param>
        /// <param name="id"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        protected ModelValidationException ValidationException<TRepresentation>(
            ModelStateDictionary modelState,
            TRepresentation content,
            Link linkTemplate,
            string message = null, int? id = null, params string[] errors)
        {
            //var metaDataProvider = Configuration.Services.GetModelMetadataProvider();
            var errorList = new List<ValidationErrorRepresentation>();

            foreach (KeyValuePair<string, ModelState> ms in modelState)
            {
                foreach (var error in ms.Value.Errors)
                {
                    //TODO: We could try to work around this but i think it's such a small thing that it's not worth spending the time fixing right now

                    ////hack - because webapi doesn't seem to support an easy way to change the model metadata for a class, we have to manually
                    //// go get the 'display' name from the metadata for the property and use that for the logref otherwise we end up with the c#
                    //// property name (i.e. contentTypeAlias vs ContentTypeAlias). I'm sure there's some webapi way to achieve 
                    //// this by customizing the model metadata but it's not as clear as with MVC which has IMetadataAware attribute
                    var logRef = ms.Key;
                    //var parts = ms.Key.Split('.');
                    //var isContentField = parts.Length == 2 && parts[0] == "content";
                    //if (isContentField)
                    //{
                    //    parts[1] = metaDataProvider.GetMetadataForProperty(() => content, typeof (ContentRepresentation), parts[1])
                    //                .GetDisplayName();
                    //    logRef = string.Join(".", parts);
                    //}

                    errorList.Add(new ValidationErrorRepresentation
                    {
                        LogRef = logRef,
                        Message = error.ErrorMessage
                    });
                }
            }
            
            //add additional messages
            foreach (var error in errors)
            {
                errorList.Add(new ValidationErrorRepresentation { Message = error });
            }

            var errorModel = new ValidationErrorListRepresentation(errorList, linkTemplate, id)
            {
                HttpStatus = (int)HttpStatusCode.BadRequest,
                Message = message ?? "Validation errors occurred"
            };

            return new ModelValidationException(errorModel);
        }

        protected void AddLocationResponseHeader(HttpResponseMessage msg, Link link)
        {
            var converter = new LinksConverter();
            msg.Headers.Add("location", converter.ResolveUri(link.Href));
        }

    }
}
