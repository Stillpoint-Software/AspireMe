namespace AspireMe.Core.Services;

[Serializable]
public class ServiceException : Exception
{
    public string Name { get; init; }
    public int Code { get; init; }

    public ServiceException()
        : base( "Service exception" )
    {
    }

    public ServiceException( string message )
        : base( message )
    {
    }

    public ServiceException( string message, Exception innerException )
        : base( message, innerException )
    {
    }

    public ServiceException( string message, int code )
        : base( message )
    {
        Code = code;
    }

    public ServiceException( string message, int code, Exception innerException )
        : base( message, innerException )
    {
        Code = code;
    }

    public ServiceException( string name, string message )
        : base( message )
    {
        Name = name;
    }

    public ServiceException( string name, string message, Exception innerException )
        : base( message, innerException )
    {
        Name = name;
    }

    public ServiceException( string name, string message, int code )
        : base( message )
    {
        Name = name;
        Code = code;
    }

    public ServiceException( string name, string message, int code, Exception innerException )
        : base( message, innerException )
    {
        Name = name;
        Code = code;
    }
}
