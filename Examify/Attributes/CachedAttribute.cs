using Examify.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

namespace Examify.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CachedAttribute : ActionFilterAttribute
    {
        private readonly int _durationMinutes;
        private readonly string _keyPrefix;

        public CachedAttribute(int durationMinutes = 5, string keyPrefix = "")
        {
            _durationMinutes = durationMinutes;
            _keyPrefix = keyPrefix;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var cacheService = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();
            var cacheKey = GenerateCacheKey(context);

            var cachedResult = cacheService.Get<JsonResult>(cacheKey);
            if (cachedResult != null)
            {
                context.Result = cachedResult;
                return;
            }

            var executedContext = await next();

            if (executedContext.Result is JsonResult jsonResult)
            {
                cacheService.Set(cacheKey, jsonResult, _durationMinutes);
            }
        }

        private string GenerateCacheKey(ActionExecutingContext context)
        {
            var keyBuilder = new StringBuilder(_keyPrefix);
            
            if (!string.IsNullOrEmpty(_keyPrefix))
                keyBuilder.Append("_");

            keyBuilder.Append($"{context.Controller.GetType().Name}_{context.ActionDescriptor.RouteValues["action"]}");

            // Add session-based institute ID if available
            var instituteId = context.HttpContext.Session.GetInt32("InstituteId");
            if (instituteId.HasValue)
                keyBuilder.Append($"_inst{instituteId}");

            // Add action parameters
            foreach (var param in context.ActionArguments.OrderBy(x => x.Key))
            {
                keyBuilder.Append($"_{param.Key}{param.Value}");
            }

            return keyBuilder.ToString();
        }
    }
}
