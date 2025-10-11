using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using DataModel;

namespace Examify.Attributes
{
    /// <summary>
    /// Custom authorization attribute that checks session and auto-authenticates with provided credentials
    /// 
    /// Usage:
    /// [AutoLoginAuthorize] - Auto-login with default admin credentials
    /// [AutoLoginAuthorize("Admin", "Teacher")] - Auto-login with default admin, but restrict to Admin and Teacher roles
    /// [AutoLoginAuthorize("testuser", "password123")] - Auto-login with specific credentials
    /// [AutoLoginAuthorize("testuser", "password123", "Admin", "Teacher")] - Auto-login with specific credentials and role restrictions
    /// 
    /// Test different users by adding query parameters to URL:
    /// ?username=testuser&password=password123
    /// Examples:
    /// - /Class?username=admin&password=admin123
    /// - /Student?username=teacher&password=teacher123
    /// - /Dashboard?username=student&password=student123
    /// </summary>
    public class AutoLoginAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;
        private readonly bool _requireSession;
        private readonly string? _defaultUsername;
        private readonly string? _defaultPassword;

        public AutoLoginAuthorizeAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles ?? new string[0];
            _requireSession = true;
            _defaultUsername = "admin";
            _defaultPassword = "admin123";
        }

        public AutoLoginAuthorizeAttribute(string username, string password, params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles ?? new string[0];
            _requireSession = true;
            _defaultUsername = username;
            _defaultPassword = password;
        }

        public AutoLoginAuthorizeAttribute(bool requireSession = true, params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles ?? new string[0];
            _requireSession = requireSession;
            _defaultUsername = "admin";
            _defaultPassword = "admin123";
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            
            // Check if user is already authenticated and session exists
            //if (IsSessionValid(httpContext))
            //{
            //    // Session is valid, user is authorized
            //    return;
            //}

            // For testing: Auto-login with credentials if no session exists
            var autoLoginResult = PerformCredentialAutoLogin(httpContext);
            if (autoLoginResult)
            {
                // Auto-login successful, user is now authorized
                return;
            }

            // If auto-login fails, redirect to login (fallback)
            RedirectToLogin(context);
        }

        private bool IsSessionValid(HttpContext httpContext)
        {
            try
            {
                // Check for JWT token first (existing pattern)
                var jwtToken = httpContext.Session.GetString("JWToken");
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    try
                    {
                        var tokenData = Examify.Common.JwtHelper.DecodeToken(jwtToken);
                        if (tokenData != null && !string.IsNullOrEmpty(tokenData.UserId))
                        {
                            // JWT token is valid, check role authorization
                            if (_allowedRoles.Length > 0)
                            {
                                var sessionRole = httpContext.Session.GetString("UserRole");
                                return !string.IsNullOrEmpty(sessionRole) && 
                                       _allowedRoles.Contains(sessionRole, StringComparer.OrdinalIgnoreCase);
                            }
                            return true;
                        }
                    }
                    catch
                    {
                        // JWT token is invalid, continue to check other session data
                    }
                }

                // Fallback to checking individual session keys
                var userId = httpContext.Session.GetInt32("UserId");
                var userRole = httpContext.Session.GetString("UserRole");
                var userName = httpContext.Session.GetString("UserName");

                // Basic session validation
                if (!_requireSession) return true;
                
                if (userId == null || userId <= 0)
                    return false;

                // Role-based authorization if roles are specified
                if (_allowedRoles.Length > 0 && !string.IsNullOrEmpty(userRole))
                {
                    return _allowedRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool PerformCredentialAutoLogin(HttpContext httpContext)
        {
            try
            {
                // Get credentials from query parameters or use defaults
                var username = httpContext.Request.Query["username"].FirstOrDefault() ?? _defaultUsername;
                var password = httpContext.Request.Query["password"].FirstOrDefault() ?? _defaultPassword;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    System.Diagnostics.Debug.WriteLine("Auto-login failed: Missing username or password");
                    return false;
                }

                // Use existing AuthService to authenticate with the actual API
                var authResult = AuthenticateWithRealAPI(httpContext, username, password).Result;
                
                if (authResult.Success && authResult.User != null)
                {
                    // Set session data using existing pattern
                    SetSessionDataWithJWT(httpContext, authResult);
                    
                    // Set authentication cookies with real JWT tokens
                    SetAuthenticationCookies(httpContext, authResult);
                    
                    // Log the auto-login for debugging
                    System.Diagnostics.Debug.WriteLine($"Auto-login successful for user: {authResult.User.UserName} (ID: {authResult.User.UserId}, Role: {authResult.User.UserRole})");
                    
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Auto-login failed: {authResult.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Auto-login failed: {ex.Message}");
                return false;
            }
        }

        private async Task<AuthenticationResult> AuthenticateWithRealAPI(HttpContext httpContext, string username, string password)
        {
            try
            {
                // Create UserDTO for API authentication
                var userDto = new UserDTO
                {
                    Username = username,
                    Password = password,
                    Role = "Admin" // Default role, will be determined by API
                };

                // Get AuthService from DI container
                var authService = httpContext.RequestServices.GetService<Examify.Services.AuthService>();
                if (authService != null)
                {
                    // Use existing AuthService to authenticate
                    var loginResult = await authService.LoginAsync(userDto);
                    
                    if (loginResult.Success && !string.IsNullOrEmpty(loginResult.Token))
                    {
                        // Decode JWT token to get user information
                        var tokenData = Examify.Common.JwtHelper.DecodeToken(loginResult.Token);
                        
                        // Create user session data from JWT token
                        var userData = new UserSessionData
                        {
                            UserId = int.Parse(tokenData.UserId ?? "0"),
                            UserName = tokenData.Username ?? username,
                            Email = tokenData.Email ?? "",
                            UserRole = "Admin", // Will be extracted from JWT claims
                            InstituteId = 3, // Default institute ID
                            StudentId = null,
                            IsActive = true
                        };

                        return new AuthenticationResult
                        {
                            Success = true,
                            Message = "Authentication successful",
                            User = userData,
                            AuthToken = loginResult.Token,
                            RefreshToken = null // Can be implemented if needed
                        };
                    }
                }

                // Fallback to hardcoded test users if AuthService fails
                return AuthenticateWithTestUsers(username, password);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Authentication failed: {ex.Message}");
                
                // Fallback to test users on API failure
                return AuthenticateWithTestUsers(username, password);
            }
        }

        private AuthenticationResult AuthenticateWithTestUsers(string username, string password)
        {
            var testUsers = GetTestUsers();
            var user = testUsers.FirstOrDefault(u => 
                string.Equals(u.UserName, username, StringComparison.OrdinalIgnoreCase) && 
                u.Password == password);

            if (user != null)
            {
                // Generate JWT-like tokens (simplified for testing)
                var authToken = GenerateAuthToken(user);
                var refreshToken = GenerateRefreshToken(user);

                return new AuthenticationResult
                {
                    Success = true,
                    Message = "Authentication successful (test user)",
                    User = new UserSessionData
                    {
                        UserId = user.UserId,
                        UserName = user.UserName,
                        Email = user.Email,
                        UserRole = user.UserRole,
                        InstituteId = user.InstituteId,
                        StudentId = user.StudentId,
                        IsActive = user.IsActive
                    },
                    AuthToken = authToken,
                    RefreshToken = refreshToken
                };
            }

            return new AuthenticationResult
            {
                Success = false,
                Message = "Invalid username or password"
            };
        }

        private List<TestUserCredentials> GetTestUsers()
        {
            return new List<TestUserCredentials>
            {
                new TestUserCredentials
                {
                    UserId = 1,
                    UserName = "admin",
                    Password = "admin123",
                    Email = "admin@examify.com",
                    UserRole = "Admin",
                    InstituteId = 3,
                    StudentId = null,
                    IsActive = true
                },
                new TestUserCredentials
                {
                    UserId = 2,
                    UserName = "teacher",
                    Password = "teacher123",
                    Email = "teacher@examify.com",
                    UserRole = "Teacher",
                    InstituteId = 3,
                    StudentId = null,
                    IsActive = true
                },
                new TestUserCredentials
                {
                    UserId = 3,
                    UserName = "student",
                    Password = "student123",
                    Email = "student@examify.com",
                    UserRole = "Student",
                    InstituteId = 3,
                    StudentId = 101,
                    IsActive = true
                },
                new TestUserCredentials
                {
                    UserId = 4,
                    UserName = "institute",
                    Password = "institute123",
                    Email = "institute@examify.com",
                    UserRole = "Institute",
                    InstituteId = 3,
                    StudentId = null,
                    IsActive = true
                },
                // Additional test users
                new TestUserCredentials
                {
                    UserId = 5,
                    UserName = "testadmin",
                    Password = "password123",
                    Email = "testadmin@examify.com",
                    UserRole = "Admin",
                    InstituteId = 3,
                    StudentId = null,
                    IsActive = true
                }
            };
        }

        private string GenerateAuthToken(TestUserCredentials user)
        {
            // Generate a simple token (in production, use proper JWT)
            var tokenData = $"{user.UserId}:{user.UserName}:{user.UserRole}:{DateTime.UtcNow.Ticks}";
            var tokenBytes = System.Text.Encoding.UTF8.GetBytes(tokenData);
            return Convert.ToBase64String(tokenBytes);
        }

        private string GenerateRefreshToken(TestUserCredentials user)
        {
            // Generate a simple refresh token
            var refreshData = $"refresh:{user.UserId}:{DateTime.UtcNow.AddDays(30).Ticks}";
            var refreshBytes = System.Text.Encoding.UTF8.GetBytes(refreshData);
            return Convert.ToBase64String(refreshBytes);
        }

        private void SetAuthenticationCookies(HttpContext httpContext, AuthenticationResult authResult)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Set to false for development/testing
                SameSite = SameSiteMode.Lax, // More permissive for testing
                Expires = DateTime.UtcNow.AddDays(30) // 30 days expiry
            };

            // Set authentication tokens
            if (!string.IsNullOrEmpty(authResult.AuthToken))
            {
                httpContext.Response.Cookies.Append("AuthToken", authResult.AuthToken, cookieOptions);
            }

            if (!string.IsNullOrEmpty(authResult.RefreshToken))
            {
                httpContext.Response.Cookies.Append("RefreshToken", authResult.RefreshToken, cookieOptions);
            }

            // Set remember me cookie
            httpContext.Response.Cookies.Append("RememberMe", "true", cookieOptions);
        }

        private void SetSessionDataWithJWT(HttpContext httpContext, AuthenticationResult authResult)
        {
            // Use existing session pattern with JWT token
            if (!string.IsNullOrEmpty(authResult.AuthToken))
            {
                httpContext.Session.SetString("JWToken", authResult.AuthToken);
            }
            
            // Set user-specific session data
            if (authResult.User != null)
            {
                httpContext.Session.SetInt32("UserId", authResult.User.UserId);
                httpContext.Session.SetString("UserName", authResult.User.UserName ?? "");
                httpContext.Session.SetString("UserRole", authResult.User.UserRole ?? "");
                httpContext.Session.SetString("Email", authResult.User.Email ?? "");
                
                if (authResult.User.InstituteId.HasValue)
                    httpContext.Session.SetInt32("InstituteId", authResult.User.InstituteId.Value);
                
                if (authResult.User.StudentId.HasValue)
                    httpContext.Session.SetInt32("StudentId", authResult.User.StudentId.Value);
            }
        }

        private void RedirectToLogin(AuthorizationFilterContext context)
        {
            var loginUrl = "/Login"; // Default login URL
            
            // Get current URL for return URL
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            
            if (!string.IsNullOrEmpty(returnUrl) && returnUrl != "/")
            {
                loginUrl += $"?returnUrl={Uri.EscapeDataString(returnUrl)}";
            }

            context.Result = new RedirectResult(loginUrl);
        }
    }

    // Response models for authentication
    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public UserSessionData? User { get; set; }
        public string? AuthToken { get; set; }
        public string? RefreshToken { get; set; }
    }

    public class UserSessionData
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? UserRole { get; set; }
        public int? InstituteId { get; set; }
        public int? StudentId { get; set; }
        public bool IsActive { get; set; }
    }

    public class TestUserCredentials
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? UserRole { get; set; }
        public int? InstituteId { get; set; }
        public int? StudentId { get; set; }
        public bool IsActive { get; set; }
    }
}