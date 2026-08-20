namespace DomainTracker.Core.Results;

public interface IResult
{
    bool Success { get; }

    int StatusCode { get; }

    List<string> Messages { get; }
}
