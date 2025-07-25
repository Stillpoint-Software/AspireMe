namespace AspireMe.Core.Services;

[Serializable]
public class ServiceOperationException : Exception
{
    private const string DefaultMessage = "Service operation exception";

    public IList<ServiceError> Errors { get; } = new List<ServiceError>();

    public ServiceOperationException()
        : base( DefaultMessage )
    {
    }

    public ServiceOperationException( string message )
        : base( message )
    {
    }

    public ServiceOperationException( string message, Exception innerException )
        : base( message, innerException )
    {
    }

    public ServiceOperationException( IList<ServiceError> errors )
        : base( DefaultMessage )
    {
        if ( errors != null )
            Errors = errors;
    }

    public ServiceOperationException( string message, IList<ServiceError> errors )
        : base( message )
    {
        if ( errors != null )
            Errors = errors;
    }

    public ServiceOperationException( string message, IList<ServiceError> errors, Exception innerException )
        : base( message, innerException )
    {
        if ( errors != null )
            Errors = errors;
    }
}
