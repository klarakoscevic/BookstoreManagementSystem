using System.Net;
using System.Text;
using System.Text.Json;

namespace BookstoreManagementSystem.Middleware;

public class AuthErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public AuthErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Store the original response body stream
        var originalBodyStream = context.Response.Body;

        // Replace the response body with a memory stream to buffer the response
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            // Call the next middleware in the pipeline
            await _next(context);

            // Check if the response status code indicates authentication or authorization failure
            // and the response hasn't started yet
            if (!context.Response.HasStarted)
            {
                if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized && responseBody.Length == 0)
                {
                    context.Response.ContentType = "application/json";
                    var response = new
                    {
                        statusCode = 401,
                        message = "Login is required for this action. Please authenticate to continue."
                    };

                    var jsonResponse = JsonSerializer.Serialize(response);
                    await responseBody.WriteAsync(Encoding.UTF8.GetBytes(jsonResponse));
                }
                else if (context.Response.StatusCode == (int)HttpStatusCode.Forbidden && responseBody.Length == 0)
                {
                    context.Response.ContentType = "application/json";
                    var response = new
                    {
                        statusCode = 403,
                        message = "You are not authorized to perform this action. Insufficient permissions."
                    };

                    var jsonResponse = JsonSerializer.Serialize(response);
                    await responseBody.WriteAsync(Encoding.UTF8.GetBytes(jsonResponse));
                }
            }

            // Copy the buffered response back to the original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        finally
        {
            // Restore the original response body stream
            context.Response.Body = originalBodyStream;
        }
    }
}
