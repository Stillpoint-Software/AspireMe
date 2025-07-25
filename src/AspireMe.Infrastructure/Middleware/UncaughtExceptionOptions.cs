namespace  AspireMe.Infrastructure.Middleware;

public class UncaughtExceptionOptions
{
    public bool LogException { get; set; } = true;
    public string Reason { get; set; } = "Internal Server Error.";
    public bool IncludeExceptionDetails { get; set; }
}
