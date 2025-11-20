using System.Net.Http.Headers;

namespace Examify.Handlers
{
    public class AuthTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthTokenHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, 
            CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var token = httpContext?.Request.Cookies["JWToken"];
            
            // Check if user is authenticated but JWT cookie is missing
            if (string.IsNullOrEmpty(token) && httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                // Try to get token from SessionScheme claims
                token = httpContext.User.FindFirst("JWToken")?.Value;
                
                if (string.IsNullOrEmpty(token)) 
                {
                    // No token in cookie or claims - return 401
                    return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent("{\"error\":\"JWT token missing\"}")
                    };
                }
            }
            
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
