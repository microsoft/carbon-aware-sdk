using CarbonAware.Exceptions;
using CarbonAware.Interfaces;

namespace CarbonAware.DataSources.Co2Signal.Client;

public class Co2SignalClientHttpException : CarbonAwareException, IHttpResponseException
{
    /// <summary>
    /// Creates a new instance of the <see cref="Co2SignalClientHttpException"/> class.
    /// </summary>
    /// <param name="message">The error message supplied.</param>
    /// <param name="response">The response object generating this exception.</param>
    public Co2SignalClientHttpException(string message, HttpResponseMessage response) : base(message)
    {
        this.Response = response;
        this.Status = (int)response.StatusCode;
        this.Title = nameof(Co2SignalClientHttpException);
        this.Detail = message;
    }

    /// <summary>
    /// Gets the status code for the exception.  See remarks for the status codes that can be returned.
    /// </summary>
    /// <remarks>
    /// 400:  Returned when missing arguments (no country code passed or lat/lon don't map to a known country code)
    /// 401:  Returned when trying to access a path or location that isn't authorized for the token.
    /// </remarks>
    public int? Status { get; }

    /// <summary>
    /// A short, human-readable summary of the problem type. It SHOULD NOT change from occurrence to occurrence
    /// of the problem, except for purposes of localization(e.g., using proactive content negotiation;
    /// see[RFC7231], Section 3.4).
    /// </summary>
    public string? Title { get; }

    /// <summary>
    /// A human-readable explanation specific to this occurrence of the problem.
    /// </summary>
    public string? Detail { get; }

    /// <summary>
    /// Gets the response returned from the Co2Signal call.
    /// </summary>
    public HttpResponseMessage? Response { get; }
}
