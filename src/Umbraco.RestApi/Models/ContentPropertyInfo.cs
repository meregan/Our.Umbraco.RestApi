namespace Umbraco.RestApi.Models
{
    public class ContentPropertyInfo
    {
        public string Label { get; set; }
        
        public bool ValidationRequired { get; set; }
        public string ValidationRegexExp { get; set; }
    }
}