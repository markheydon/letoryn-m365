using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace TenancyHub.ApiService.UnitTests.Infrastructure;

internal static class HttpResultTestHelper
{
    public static async Task<(int StatusCode, string ContentType, string Body)> ExecuteAsync(IResult result)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.RequestServices = new ServiceCollection()
            .AddLogging()
            .AddProblemDetails()
            .BuildServiceProvider();

        await result.ExecuteAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();

        return (context.Response.StatusCode, context.Response.ContentType ?? string.Empty, body);
    }

    public static JsonDocument ParseJsonBody(string body) => JsonDocument.Parse(body);
}
