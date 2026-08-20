namespace DomainTracker.Core.Results;

public class ErrorResult : Result
{
    public ErrorResult(int statusCode, string message) : base(false, statusCode, message)
    {
    }

    public ErrorResult(int statusCode, List<string> messages) : base(false, statusCode, messages)
    {
    }
}
