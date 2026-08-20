namespace DomainTracker.Core.Results;

public class ErrorDataResult<T> : DataResult<T>
{
    public ErrorDataResult(int statusCode, string message) : base(default, false, statusCode, message)
    {
    }

    public ErrorDataResult(int statusCode, List<string> messages) : base(default, false, statusCode, messages)
    {
    }
}
