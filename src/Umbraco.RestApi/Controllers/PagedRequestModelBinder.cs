using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Umbraco.RestApi.Models;

namespace Umbraco.RestApi.Controllers
{
    public class PagedRequestModelBinder : IModelBinder
    {
        /// <summary>
        /// Binds the model to a value by using the specified controller context and binding context.
        /// </summary>
        /// <returns>
        /// true if model binding is successful; otherwise, false.
        /// </returns>
        /// <param name="actionContext">The action context.</param><param name="bindingContext">The binding context.</param>
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            //create a dictionary with these query strings - in our case, all query strings are unique so we always override the key
            var queryStrings = new Dictionary<string, string>();
            foreach (var queryNameValuePair in actionContext.Request.GetQueryNameValuePairs())
            {
                queryStrings[queryNameValuePair.Key] = queryNameValuePair.Value;
            }
            return PerformBindModel(queryStrings, actionContext, bindingContext);
        }

        protected virtual bool PerformBindModel(IDictionary<string, string> queryStrings, HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            var queryStructure = new PagedRequest();

            if (queryStrings.TryGetValue("size", out var qsVal) && int.TryParse(qsVal, out var pageSize))
            {
                queryStructure.PageSize = pageSize;
            }

            if (queryStrings.TryGetValue("page", out qsVal) && int.TryParse(qsVal, out var page))
            {
                queryStructure.Page = page;
            }

            bindingContext.Model = queryStructure;
            return true;
        }
    }
}