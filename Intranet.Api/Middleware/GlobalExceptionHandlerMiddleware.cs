using Microsoft.AspNetCore.Mvc;
using System.Data.Common;
using System.Net;
using System.Text.Json;

namespace Intranet.Api.Middleware
{
    public class GlobalExceptionHandlerMiddleware : IMiddleware
    {
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandlerMiddleware(ILogger<GlobalExceptionHandlerMiddleware> logger, IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {ErrorMessage}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/problem+json"; // RFC 7807
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError; // Default
            string title = "An unexpected error occurred.";
            string detail = exception.Message; // Default detail

            // Customize based on exception type
            if (exception is DbException dbEx) // Catch specific database exceptions if needed
            {
                title = "A database error occurred.";
                // Avoid leaking sensitive details in production
                detail = _env.IsDevelopment() ? dbEx.ToString() : "An error occurred while processing your request.";
                statusCode = HttpStatusCode.InternalServerError; // Or perhaps BadGateway if appropriate
            }
            else if (exception is ArgumentException argEx) // Example: Bad input mapping
            {
                title = "Invalid request argument.";
                detail = argEx.Message;
                statusCode = HttpStatusCode.BadRequest;
            }
            else if (exception is InvalidOperationException opEx) // Example: Business logic violation
            {
                title = "Invalid operation.";
                detail = opEx.Message;
                statusCode = HttpStatusCode.Conflict; // Or BadRequest depending on context
            }
            // Add more specific exception handling here (e.g., NotFoundException, UnauthorizedAccessException)

            context.Response.StatusCode = (int)statusCode;

            var problemDetails = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = title,
                Detail = _env.IsDevelopment() ? exception.ToString() : detail, // Show full stack trace only in Development
                Instance = context.Request.Path // Identifies the specific request URI
            };

            // Add trace ID for correlation
            problemDetails.Extensions["traceId"] = System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier;

            // Log the detailed problem
            _logger.LogError("Responding with ProblemDetails: {@ProblemDetails}", problemDetails);

            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }
    }
}
