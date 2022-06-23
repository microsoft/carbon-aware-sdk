using CarbonAware.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Diagnostics;

namespace CarbonAware.WebApi.Filters;

public class HttpResponseExceptionFilter : IExceptionFilter
{
    private ILogger<HttpResponseExceptionFilter> _logger;
    private IConfiguration config;

    private static Dictionary<string, int> EXCEPTION_STATUS_CODE_MAP = new Dictionary<string, int>()
    {
        { "ArgumentException", (int)HttpStatusCode.BadRequest },
        { "NotImplementedException", (int)HttpStatusCode.NotImplemented },
    };

    public HttpResponseExceptionFilter(ILogger<HttpResponseExceptionFilter> logger, IConfiguration configuration)
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
                _logger.LogError(context.Exception, "500 Error: Unhandled exception");
            }
            var envVars = config?.GetSection(CarbonAwareVariablesConfiguration.Key).Get<CarbonAwareVariablesConfiguration>();
            var isVerboseApi = Convert.ToBoolean(envVars?.VerboseApi);
           
            if (isVerboseApi)
            {
                response =  CreateValidationProblemDetails(exceptionType, statusCode, context.Exception.Message);
                response.Errors["trace"] = new string[1] { context.Exception.StackTrace! };
            }
            else 
            {
                response =  CreateValidationProblemDetails("InternalServerError", statusCode, "");
            }
            
        }

        var traceId = activity?.Id;
        if (traceId != null)
        {
            response.Extensions["traceId"] = traceId;
            response.Errors["trace"] = new string[1] {context.Exception.StackTrace!};

        }

        context.Result = new ObjectResult(response)
        {
            StatusCode = response.Status
        };

        context.ExceptionHandled = true;
    }

    private HttpValidationProblemDetails CreateValidationProblemDetails(string exceptionType, int statusCode, string message)
    {
        var response = new HttpValidationProblemDetails()
            {
                Title = exceptionType,
                Status = statusCode,
                Detail = message
            };
        return response;    
    }
}