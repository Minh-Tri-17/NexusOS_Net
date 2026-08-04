using Microsoft.Extensions.Localization;
using NexusOS.MB;
using NexusOS.Util;
using System.Net;
using System.Text.Json;

namespace NexusOS.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next; // Dùng để chuyển tiếp request tiếp theo
        private readonly ILogger<ExceptionMiddleware> _logger; // Dùng để ghi log hệ thống
        private readonly IStringLocalizer _localizer; // Dùng để đa ngôn ngữ hóa thông báo

        public ExceptionMiddleware(RequestDelegate next, IStringLocalizer localizer, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var result = new APIResults<string>
                {
                    IsSuccess = false,
                    Message = _localizer[Messages.InternalServerError]
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(result));
            }
        }
    }
}
