using FluentValidation.Results;

namespace DomainTracker.Business.Validation
{
    internal static class ValidationResultExtensions
    {
        public static List<string> ToMessages(this ValidationResult result) =>
            result.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}").ToList();
    }
}
