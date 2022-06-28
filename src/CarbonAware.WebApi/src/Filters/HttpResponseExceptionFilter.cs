using CarbonAware.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace CarbonAware.WebApi.Filters;

public class HttpResponseExceptionFilter : IExceptionFilter
{
    private ILogger<HttpResponseExceptionFilter> _logger;
    private IOptionsMonitor<CarbonAwareVariablesConfiguration> config;

    private static Dictionary<string, int> EXCEPTION_STATUS_CODE_MAP = new Dictionary<string, int>()
    {
        { "ArgumentException", (int)HttpStatusCode.BadRequest },
        { "NotImplementedException", (int)HttpStatusCode.NotImplemented },
    };

    public HttpResponseExceptionFilter(ILogger<HttpResponseExceptionFilter> logger, IOptionsMonitor<CarbonAwareVariablesConfiguration> configuration)
    {
        _logger = logger;
        config = configuration;
    }

    public void OnException(ExceptionContext context)
    {
        var activity = Activity.Current;

        HttpValidationProblemDetails response;
        if (context.Exception is IHttpResponseException httpResponseException)
        {
            response = new HttpValidationProblemDetails(){
                Title = httpResponseException.Title,
                Status = httpResponseException.Status,
                Detail = httpResponseException.Detail
            };
        } else {
            var exceptionType = context.Exception.GetType().Name;
            int statusCode;
            if (!EXCEPTION_STATUS_CODE_MAP.TryGetValue(exceptionType, out statusCode))
            {
                statusCode = (int)HttpStatusCode.InternalServerError;
                activity?.SetStatus(ActivityStatusCode.Error, context.Exception.Message);
            }
            var isVerboseApi = config?.CurrentValue.VerboseApi;
       
            if (statusCode == (int)HttpStatusCode.InternalServerError && isVerboseApi == false)
            {
                 response = new HttpValidationProblemDetails() {
                                Title = HttpStatusCode.InternalServerError.ToString(),
                                Status = statusCode,
                                Detail = context.Exception.Message
                    };
            }
            else
            {
                response = new HttpValidationProblemDetails() {
                            Title = exceptionType,
                            Status = statusCode,
                            Detail = context.Exception.Message
                };
                if (isVerboseApi == true) {
                    response.Errors["stackTrace"] = new string[] { context.Exception.StackTrace! };
                }
            }
        }

        var traceId = activity?.Id;
        if (traceId != null)
        {
            response.Extensions["traceId"] = traceId;
        }

        context.Result = new ObjectResult(response)
        {
            StatusCode = response.Status
        };
        _logger.LogError(context.Exception, "Exception: {exception}", context.Exception.Message);
        context.ExceptionHandled = true;
    }
}
