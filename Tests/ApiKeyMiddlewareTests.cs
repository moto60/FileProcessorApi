using FileProcessorApi.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FileProcessorApi.Tests;

public sealed class ApiKeyMiddlewareTests
{
    private const string ValidApiKey = "test-api-key";

    [Fact]
    public async Task InvokeAsync_ContinuesWhenApiKeyIsValid()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = CreateProtectedContext();
        context.Request.Headers[ApiKeyMiddleware.HeaderName] = ValidApiKey;

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("wrong-key")]
    public async Task InvokeAsync_RejectsMissingOrInvalidApiKey(string? apiKey)
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = CreateProtectedContext();
        if (apiKey is not null)
            context.Request.Headers[ApiKeyMiddleware.HeaderName] = apiKey;

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.False(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_RejectsMultipleApiKeyHeaders()
    {
        var middleware = CreateMiddleware(_ => Task.CompletedTask);
        var context = CreateProtectedContext();
        context.Request.Headers[ApiKeyMiddleware.HeaderName] =
            new[] { ValidApiKey, "another-key" };

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsServerErrorWhenApiKeyIsNotConfigured()
    {
        var middleware = CreateMiddleware(_ => Task.CompletedTask, configuredApiKey: null);
        var context = CreateProtectedContext();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_DoesNotRequireKeyForUnprotectedEndpoint()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.NotEqual(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    private static ApiKeyMiddleware CreateMiddleware(
        RequestDelegate next,
        string? configuredApiKey = ValidApiKey)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [ApiKeyMiddleware.ConfigurationKey] = configuredApiKey
            })
            .Build();

        return new ApiKeyMiddleware(next, configuration, NullLogger<ApiKeyMiddleware>.Instance);
    }

    private static DefaultHttpContext CreateProtectedContext()
    {
        var context = new DefaultHttpContext();
        context.SetEndpoint(new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(new ApiKeyRequiredAttribute()),
            "protected"));

        return context;
    }
}
