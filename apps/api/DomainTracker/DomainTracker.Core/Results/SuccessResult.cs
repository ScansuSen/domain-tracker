using DomainTracker.Core.Constants;

namespace DomainTracker.Core.Results;

public class SuccessResult : Result
{
    public SuccessResult(int statusCode = HttpStatusCodes.Ok) : base(true, statusCode)
    {
    }

    public SuccessResult(int statusCode, string message) : base(true, statusCode, message)
    {
    }
}
