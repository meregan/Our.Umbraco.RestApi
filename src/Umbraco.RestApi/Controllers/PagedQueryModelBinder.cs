using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Umbraco.RestApi.Models;

namespace Umbraco.RestApi.Controllers
{
    public class PagedQueryModelBinder : PagedRequestModelBinder
    {
        /// <inheritdoc />
        protected override bool PerformBindModel(IDictionary<string, string> queryStrings, HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            var result = base.PerformBindModel(queryStrings, actionContext, bindingContext);
            if (!result) return false;

            var model = (PagedRequest)bindingContext.Model;
            var queryStructure = new PagedQuery
            {
                Page = model.Page,
                PageSize = model.PageSize
            };
            
            if (queryStrings.TryGetValue("query", out var qsVal))
            {
                queryStructure.Query = qsVal;
            }

            bindingContext.Model = queryStructure;
            return true;
        }
    }
}