using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class ValidationErrorRepresentation : Representation
    {
        public string Message { get; set; }
        public string LogRef { get; set; }

        public override string Rel
        {
            get { return "errors"; }
            set { }
        }

        public override string Href
        {
            get
            {
                //TODO: We could make this dynamic in a way that supplies a different link
                // for different error types (i.e. fields vs properties)
                return "http://our.umbraco.org/documentation/";
            }
            set { }
        }
    }
}