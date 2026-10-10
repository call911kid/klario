using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Klario.BLL.Exceptions;
using Klario.PL.Constants;
using Klario.PL.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Klario.PL.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred while processing the request.");
                await HandleExceptionAsync(context, ex);
            }
        }

        public async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = GetStatusCode(exception);
            var errorCode = GetErrorCode(exception);
            var message = GetMessage(exception);
            var errors = GetErrors(exception);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsJsonAsync(ApiResponse.Failure(errorCode, message, new List<string>(errors)));
        }

        private static string GetErrorCode(Exception exception) => exception switch
        {
            EntityNotFoundException => ErrorCodes.EntityNotFound,
            UnauthorizedAccessException => ErrorCodes.Unauthorized,
            ArgumentException or InvalidOperationException => ErrorCodes.BadRequest,
            _ => ErrorCodes.InternalServerError
        };

        private static HttpStatusCode GetStatusCode(Exception exception) => exception switch
        {
            EntityNotFoundException => HttpStatusCode.NotFound,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            ArgumentException or InvalidOperationException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        private static string GetMessage(Exception exception) => exception switch
        {
            EntityNotFoundException entityNotFoundException => entityNotFoundException.Message,
            UnauthorizedAccessException unauthorizedException => unauthorizedException.Message,
            ArgumentException argumentException => argumentException.Message,
            _ => "An unexpected error occurred."
        };

        private static IEnumerable<string> GetErrors(Exception exception) => exception switch
        {
            _ => Array.Empty<string>()
        };
    }
}
