using DomainTracker.Core.Constants;

namespace DomainTracker.Core.Results;

public class SuccessDataResult<T> : DataResult<T>
{
    public SuccessDataResult(T data, int statusCode = HttpStatusCodes.Ok) : base(data, true, statusCode)
    {
    }

    public SuccessDataResult(T data, int statusCode, string message) : base(data, true, statusCode, message)
    {
    }
}
