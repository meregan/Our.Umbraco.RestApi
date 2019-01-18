using System;
using Umbraco.RestApi.Models;

namespace Umbraco.RestApi
{
    public class ModelValidationException : Exception
    {
        public ValidationErrorListRepresentation Errors { get; private set; }
        public int? Id { get; set; }

        public ModelValidationException(ValidationErrorListRepresentation errors, int? id = null)
        {
            Errors = errors;
            Id = id;
        }
    }
}
