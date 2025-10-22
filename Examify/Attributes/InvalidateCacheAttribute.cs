using Examify.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Examify.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class InvalidateCacheAttribute : ActionFilterAttribute
    {
        private readonly string _pattern;

        public InvalidateCacheAttribute(string pattern)
        {
            _pattern = pattern;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var cacheService = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();
            cacheService.RemoveByPattern(_pattern);
            base.OnActionExecuted(context);
        }
    }
}
