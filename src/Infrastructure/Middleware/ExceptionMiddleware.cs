using System.Net;
using Demo.WebApi.Application.Common.Enums;
using Demo.WebApi.Application.Common.Exceptions;
using Demo.WebApi.Application.Common.Interfaces;
using Demo.WebApi.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Serilog;
using Serilog.Context;

namespace Demo.WebApi.Infrastructure.Middleware;

internal class ExceptionMiddleware : IMiddleware
{
    private readonly ICurrentUser _currentUser;
    private readonly IStringLocalizer _t;
    private readonly ISerializerService _jsonSerializer;

    public ExceptionMiddleware(
        ICurrentUser currentUser,
        IStringLocalizer<ExceptionMiddleware> localizer,
        ISerializerService jsonSerializer)
    {
        _currentUser = currentUser;
        _t = localizer;
        _jsonSerializer = jsonSerializer;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            string email = _currentUser.GetUserEmail() is string userEmail ? userEmail : "Anonymous";
            var userId = _currentUser.GetUserId();
            if (userId != Guid.Empty) LogContext.PushProperty("UserId", userId);
            LogContext.PushProperty("UserEmail", email);
            string errorId = Guid.NewGuid().ToString();
            LogContext.PushProperty("ErrorId", errorId);
            LogContext.PushProperty("StackTrace", exception.StackTrace);
            var errorResult = new HttpResponseMetadata
            {
                Message = exception.Message.Trim(),
                Type = HttpResponseType.Error.ToString(),
                ErrorId = errorId,
            };

            var response = context.Response;
            switch (exception)
            {
                case CustomException e:
                    response.StatusCode = errorResult.StatusCode = (int)e.StatusCode;
                    if (e.ErrorMessages is not null)
                    {
                        errorResult.Message = string.Join(Environment.NewLine, e.ErrorMessages);
                    }

                    break;

                case KeyNotFoundException:
                    response.StatusCode = errorResult.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                default:
                    response.StatusCode = errorResult.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            Log.Error($"{errorResult.Message} Request failed with Status Code {errorResult.StatusCode} and Error Id {errorId}.");

            HttpResponseDto<object> httpResponse = new HttpResponseDto<object>
            {
                Metadata = errorResult,
            };

            if (!response.HasStarted)
            {
                response.ContentType = "application/json";
                await response.WriteAsync(_jsonSerializer.Serialize(httpResponse));
            }
            else
            {
                Log.Warning("Can't write error response. Response has already started.");
            }
        }
    }
}