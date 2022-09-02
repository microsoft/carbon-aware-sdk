using CarbonAware.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;

namespace CarbonAware.CLI.Extensions;
public static class CommandLineBuilderExtensions
{
    public enum ExitCode
    {
        Success = 0,
        Failure = 1,
        InvalidArguments = 2,
        DataSourceError = 3,
    }

    public static CommandLineBuilder UseCarbonAwareExceptionHandler(this CommandLineBuilder builder)
    {
        return builder.UseExceptionHandler(ExceptionHandler);
    }

    private static void ExceptionHandler(Exception exception, InvocationContext context)
    {
        var exitCode = ExitCode.Failure;
        if (exception is IHttpResponseException httpResponseException)
        {
            context.Console.Error.Write($"{httpResponseException.Title}\n".Red().Bold());
            context.Console.Error.Write($"{httpResponseException.Status}\n".Red());
            context.Console.Error.Write($"{httpResponseException.Detail}\n".Red());
            exitCode = ExitCode.DataSourceError;
        } else if (exception is ArgumentException){
            context.Console.Error.Write($"{exception.Message}\n".Red().Bold());
            foreach (DictionaryEntry entry in exception.Data)
            {
                if (entry.Value is string[] messages && entry.Key is string key){
                    context.Console.Error.Write($"{key}: ".Red().Bold());
                    foreach (var message in messages)
                    {
                        context.Console.Error.Write($"{message}\n".Red());
                    }
                }
            }
            exitCode = ExitCode.InvalidArguments;
        } else {
            context.Console.Error.Write($"{exception.Message}\n".Red());
        }
        context.ExitCode = (int)exitCode;
    }
}