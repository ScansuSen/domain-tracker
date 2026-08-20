namespace DomainTracker.Core.Results;

public abstract class DataResult<T> : Result, IDataResult<T>
{
    protected DataResult(T? data, bool success, int statusCode) : base(success, statusCode)
    {
        Data = data;
    }

    protected DataResult(T? data, bool success, int statusCode, string message) : base(success, statusCode, message)
    {
        Data = data;
    }

    protected DataResult(T? data, bool success, int statusCode, List<string> messages) : base(success, statusCode, messages)
    {
        Data = data;
    }

    public T? Data { get; }
}
