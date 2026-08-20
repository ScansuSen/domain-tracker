namespace DomainTracker.Core.Results;

public abstract class Result : IResult
{
    protected Result(bool success, int statusCode)
    {
        Success = success;
        StatusCode = statusCode;
        Messages = [];
    }

    protected Result(bool success, int statusCode, string message) : this(success, statusCode)
    {
        Messages.Add(message);
    }

    protected Result(bool success, int statusCode, List<string> messages) : this(success, statusCode)
    {
        Messages = messages;
    }

    public bool Success { get; }

    public int StatusCode { get; }

    public List<string> Messages { get; }
}
