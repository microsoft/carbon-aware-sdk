using CarbonAware.WebApi.Filters;
using CarbonAware.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace CarbonAware.WepApi.UnitTests;

[TestFixture]
public class HttpResponseExceptionFilterTests
{
    // Not relevant for tests which populate this field via the [SetUp] attribute.
    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private ActionContext _actionContext;
    private Mock<ILogger<HttpResponseExceptionFilter>> _logger;

    private IConfiguration _config;

    #pragma warning restore CS8618

    [SetUp]
    public void Setup()
    {
        this._actionContext = new ActionContext()
        {
            HttpContext = new DefaultHttpContext(),
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor()
        };
        this._logger = new Mock<ILogger<HttpResponseExceptionFilter>>();
    }

    [Test]
    public void TestOnException_IHttpResponseException()
    {
        // Arrange
        var ex = new DummyHttpResponseException();
        var exceptionContext = new ExceptionContext(this._actionContext, new List<IFilterMetadata>())
        {
            Exception = ex
        };
        var filter = new HttpResponseExceptionFilter(this._logger.Object, this._config);

        // Act
        filter.OnException(exceptionContext);
        var result = exceptionContext.Result as ObjectResult ?? throw new Exception();
        var content = result.Value as HttpValidationProblemDetails ?? throw new Exception();

        // Assert
        Assert.IsTrue(exceptionContext.ExceptionHandled);
        Assert.AreEqual(ex.Status, result.StatusCode);
        Assert.AreEqual(ex.Status, content.Status);
        Assert.AreEqual(ex.Title, content.Title);
        Assert.AreEqual(ex.Detail, content.Detail);
    }

    [Test]
    public void TestOnException_ArgumentException()
    {
        // Arrange
        var ex = new ArgumentException("My validation error");
        var exceptionContext = new ExceptionContext(this._actionContext, new List<IFilterMetadata>())
        {
            Exception = ex
        };
        var filter = new HttpResponseExceptionFilter(this._logger.Object, this._config);

        // Act
        filter.OnException(exceptionContext);

        // Assert
        var result = exceptionContext.Result as ObjectResult ?? throw new Exception();
        var content = result.Value as HttpValidationProblemDetails ?? throw new Exception();

        Assert.IsTrue(exceptionContext.ExceptionHandled);
        Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
        Assert.AreEqual((int)HttpStatusCode.BadRequest, content.Status);
        Assert.AreEqual("ArgumentException", content.Title);
        Assert.AreEqual("My validation error", content.Detail);
    }

    [Test]
    public void TestOnException_NotImplementedException()
    {
        // Arrange
        var ex = new NotImplementedException("My validation error");
        var exceptionContext = new ExceptionContext(this._actionContext, new List<IFilterMetadata>())
        {
            Exception = ex
        };

        var filter = new HttpResponseExceptionFilter(this._logger.Object, this._config);

        // Act
        filter.OnException(exceptionContext);

        // Assert
        var result = exceptionContext.Result as ObjectResult ?? throw new Exception();
        var content = result.Value as HttpValidationProblemDetails ?? throw new Exception();

        Assert.IsTrue(exceptionContext.ExceptionHandled);
        Assert.AreEqual((int)HttpStatusCode.NotImplemented, result.StatusCode);
        Assert.AreEqual((int)HttpStatusCode.NotImplemented, content.Status);
        Assert.AreEqual("NotImplementedException", content.Title);
        Assert.AreEqual("My validation error", content.Detail);
    }

    [Test]
    public void TestOnException_GenericException()
    {
        // Arrange
        var ex = new Exception("My validation error");
        var exceptionContext = new ExceptionContext(this._actionContext, new List<IFilterMetadata>())
        {
            Exception = ex
        };

        var filter = new HttpResponseExceptionFilter(this._logger.Object, this._config);

        // Act
        filter.OnException(exceptionContext);

        // Assert
        var result = exceptionContext.Result as ObjectResult ?? throw new Exception();
        var content = result.Value as HttpValidationProblemDetails ?? throw new Exception();

        Assert.IsTrue(exceptionContext.ExceptionHandled);
        Assert.AreEqual((int)HttpStatusCode.InternalServerError, result.StatusCode);
        Assert.AreEqual((int)HttpStatusCode.InternalServerError, content.Status);
        Assert.AreEqual("Exception", content.Title);
        Assert.AreEqual("My validation error", content.Detail);
    }

    [Test]
    public void TestOnException_GenericException_WithVerboseTrue()
    {
        var inMemorySettings = new Dictionary<string, string> {
            { $"{CarbonAwareVariablesConfiguration.Key}:VerboseApi", "true" }
        };
        this._config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Arrange
        var ex = new Exception("My validation error");
        var exceptionContext = new ExceptionContext(this._actionContext, new List<IFilterMetadata>())
        {
            Exception = ex
        };
        
        var filter = new HttpResponseExceptionFilter(this._logger.Object, this._config);

        // Act
        filter.OnException(exceptionContext);

        var result = exceptionContext.Result as ObjectResult ?? throw new Exception();
        var content = result.Value as HttpValidationProblemDetails ?? throw new Exception();

        // Assert
        Assert.IsTrue(exceptionContext.ExceptionHandled);
        Assert.AreEqual((int)HttpStatusCode.InternalServerError, result.StatusCode);
        Assert.AreEqual("Exception", content.Title);
        Assert.AreEqual("My validation error", content.Detail);
        Assert.IsNotEmpty(content.Errors);

    }

    [Test]
    public void TestOnException_GenericException_WithVerboseFalse()
    {
        var inMemorySettings = new Dictionary<string, string> {
            { $"{CarbonAwareVariablesConfiguration.Key}:VerboseApi", "false" }
        };
        this._config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Arrange
        var ex = new Exception("My validation error");
        var exceptionContext = new ExceptionContext(this._actionContext, new List<IFilterMetadata>())
        {
            Exception = ex
        };
        
        var filter = new HttpResponseExceptionFilter(this._logger.Object, this._config);

        // Act
        filter.OnException(exceptionContext);

        var result = exceptionContext.Result as ObjectResult ?? throw new Exception();
        var content = result.Value as HttpValidationProblemDetails ?? throw new Exception();

        // Assert
        Assert.IsTrue(exceptionContext.ExceptionHandled);
        Assert.AreEqual((int)HttpStatusCode.InternalServerError, result.StatusCode);
        Assert.AreEqual(HttpStatusCode.InternalServerError.ToString(), content.Title);
        Assert.AreEqual("My validation error", content.Detail);
        Assert.IsEmpty(content.Errors);
    }
}

public class DummyHttpResponseException : Exception, IHttpResponseException
{
    public string? Title => "Dummy Title";
    public string? Detail => "Dummy Details";
    public int? Status => 418;
    public HttpResponseMessage? Response => null;
}
