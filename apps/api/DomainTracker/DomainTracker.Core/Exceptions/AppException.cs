namespace DomainTracker.Core.Exceptions;

public abstract class AppException : Exception
{
    public int StatusCode { get; }

    public IDictionary<string, string[]>? Errors { get; }

    protected AppException(string message, int statusCode, IDictionary<string, string[]>? errors = null)
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }
}
