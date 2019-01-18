using AutoMapper;
using WebApi.Hal;

namespace Umbraco.RestApi.Models.Mapping
{
    internal static class MappingExpressionExtensions
    {
        /// <summary>
        /// Use with care! this will ignore everything unless explicitly set
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDest"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static IMappingExpression<TSource, TDest> IgnoreAllUnmapped<TSource, TDest>(this IMappingExpression<TSource, TDest> expression)
        {
            expression.ForAllMembers(opt => opt.Ignore());
            return expression;
        }

        public static IMappingExpression<TSource, TDest> IgnoreHalProperties<TSource, TDest>(this IMappingExpression<TSource, TDest> expression)
            where TDest: Representation
        {
            expression.ForMember(representation => representation.Href, opt => opt.Ignore());
            expression.ForMember(representation => representation.Rel, opt => opt.Ignore());
            expression.ForMember(representation => representation.LinkName, opt => opt.Ignore());
            expression.ForMember(representation => representation.Links, opt => opt.Ignore());
            return expression;
        }
    }
}