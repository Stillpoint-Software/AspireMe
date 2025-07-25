using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspireMe.Infrastructure.Middleware;

public class UncaughtExceptionMiddleware
{
    private readonly ILogger<UncaughtExceptionMiddleware> _logger;
    private readonly UncaughtExceptionOptions _options;
    private readonly RequestDelegate _next;

    private record ExceptionStatus
    {
        public int StatusCode { get; init; }
        public string ExceptionDetails { get; init; }
        public string ExceptionMessage { get; init; }
    }

    public UncaughtExceptionMiddleware( RequestDelegate next, ILogger<UncaughtExceptionMiddleware> logger, UncaughtExceptionOptions options )
    {
        _logger = logger;
        _options = options;
        _next = next;
    }

    public async Task InvokeAsync( HttpContext httpContext ) // name must match this signature
    {
        try
        {
            await _next( httpContext ).ConfigureAwait( false );
        }
        catch ( Exception ex )
        {
            await HandleExceptionAsync( httpContext, ex ).ConfigureAwait( false );
        }
    }

    private Task HandleExceptionAsync( HttpContext context, Exception exception )
    {
        string Reason( int code )
        {
            var reasonPhrase = ReasonPhrases.GetReasonPhrase( code );
            return !string.IsNullOrEmpty( reasonPhrase )
                ? reasonPhrase
                : _options.Reason;
        }

        if ( _options.LogException )
            _logger.LogError( exception, "Unhandled exception" );

        var exceptionStatus = GetExceptionStatus( exception );

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = exceptionStatus.StatusCode;

        var content = JsonSerializer.Serialize( new
        {
            context.Response.StatusCode,
            Reason = Reason( exceptionStatus.StatusCode ),
            Details = _options.IncludeExceptionDetails
                ? exceptionStatus.ExceptionDetails
                : exceptionStatus.ExceptionMessage
        } );

        return context.Response.WriteAsync( content );
    }

    private static ExceptionStatus GetExceptionStatus( Exception exception, HttpStatusCode defaultStatusCode = HttpStatusCode.InternalServerError )
    {
        return exception switch
        {
            BadHttpRequestException badHttpRequestException => new ExceptionStatus
            {
                StatusCode = badHttpRequestException.StatusCode,
                ExceptionDetails = badHttpRequestException.ToString(),
                ExceptionMessage = badHttpRequestException.Message
            },
            HttpRequestException httpRequestException => new ExceptionStatus
            {
                StatusCode = (int) (httpRequestException.StatusCode ?? defaultStatusCode),
                ExceptionDetails = httpRequestException.ToString(),
                ExceptionMessage = httpRequestException.Message
            },
            _ => new ExceptionStatus
            {
                StatusCode = (int) defaultStatusCode,
                ExceptionDetails = exception.ToString(),
                ExceptionMessage = exception.Message
            }
        };
    }
}

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseUncaughtExceptionHandler( this IApplicationBuilder app, Action<UncaughtExceptionOptions> configure = null )
    {
        var options = app.ApplicationServices.GetRequiredService<IOptions<UncaughtExceptionOptions>>()?.Value ?? new UncaughtExceptionOptions();

        configure?.Invoke( options );

        app.UseMiddleware<UncaughtExceptionMiddleware>( options );

        return app;
    }
}
