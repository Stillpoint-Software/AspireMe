namespace AspireMe.Core.Services;

public class ServiceError
{
    public string Message { get; init; }
    public int Code { get; init; }
    public string Name { get; init; }
    public SeverityLevel Severity { get; init; }

    public ServiceError()
    {
    }

    public ServiceError( string message )
    {
        Message = message;
    }
}
