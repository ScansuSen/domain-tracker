using DomainTracker.Core.Constants;

namespace DomainTracker.Core.Exceptions;

public class ExternalServiceException : AppException
{
    public ExternalServiceException(string message) : base(message, HttpStatusCodes.BadGateway)
    {
    }
}
