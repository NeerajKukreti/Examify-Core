using ExamifyAPI.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Check if a username already exists
    /// </summary>
    /// <param name="userName">Username to check</param>
    /// <param name="userId">Optional: UserId to exclude from check (for edit scenarios)</param>
    /// <returns>Returns whether the username exists</returns>
    [HttpGet("check-username")]
    public async Task<IActionResult> CheckUserNameExists([FromQuery] string userName, [FromQuery] int? userId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return BadRequest(new { 
                    Success = false, 
                    Message = "Username is required" 
                });
            }

            var exists = await _userService.CheckUserNameExistsAsync(userName, userId);
            
            return Ok(new { 
                Success = true,
                UserName = userName,
                Exists = exists,
                Message = exists ? "Username already exists" : "Username is available"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                Success = false, 
                Message = "An error occurred while checking username availability.",
                Error = ex.Message 
            });
        }
    }

    /// <summary>
    /// Validate username availability for real-time validation
    /// </summary>
    /// <param name="request">Username validation request</param>
    /// <returns>Returns validation result</returns>
    [HttpPost("validate-username")]
    public async Task<IActionResult> ValidateUserName([FromBody] ValidateUserNameRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserName))
            {
                return BadRequest(new { 
                    Success = false,
                    IsValid = false,
                    Message = "Username is required" 
                });
            }

            // Check basic username requirements
            if (request.UserName.Length < 3)
            {
                return Ok(new { 
                    Success = true,
                    IsValid = false,
                    Message = "Username must be at least 3 characters long"
                });
            }

            if (request.UserName.Length > 50)
            {
                return Ok(new { 
                    Success = true,
                    IsValid = false,
                    Message = "Username cannot exceed 50 characters"
                });
            }

            // Check if username exists
            var exists = await _userService.CheckUserNameExistsAsync(request.UserName, request.UserId);
            
            if (exists)
            {
                return Ok(new { 
                    Success = true,
                    IsValid = false,
                    Message = "Username is already taken"
                });
            }

            return Ok(new { 
                Success = true,
                IsValid = true,
                Message = "Username is available"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                Success = false,
                IsValid = false,
                Message = "An error occurred while validating username.",
                Error = ex.Message 
            });
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { 
                    Success = false, 
                    Message = "User not found" 
                });
            }

            // Don't return password hash for security
            var userResponse = new
            {
                UserId = user.UserId,
                Username = user.Username,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate
            };

            return Ok(new { 
                Success = true, 
                Data = userResponse 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                Success = false, 
                Message = "An error occurred while retrieving user.",
                Error = ex.Message 
            });
        }
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="request">User creation request</param>
    /// <returns>Created user ID</returns>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username) || 
                string.IsNullOrWhiteSpace(request.Password) || 
                string.IsNullOrWhiteSpace(request.Role))
            {
                return BadRequest(new { 
                    Success = false, 
                    Message = "Username, password, and role are required" 
                });
            }

            // Check if username already exists
            var exists = await _userService.CheckUserNameExistsAsync(request.Username);
            if (exists)
            {
                return BadRequest(new { 
                    Success = false, 
                    Message = "Username already exists" 
                });
            }

            var userId = await _userService.CreateUserAsync(request.Username, request.Password, request.Role);
            
            return CreatedAtAction(nameof(GetUser), new { id = userId }, new { 
                Success = true, 
                UserId = userId, 
                Message = "User created successfully" 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                Success = false, 
                Message = "An error occurred while creating user.",
                Error = ex.Message 
            });
        }
    }
}

/// <summary>
/// Request model for username validation
/// </summary>
public class ValidateUserNameRequest
{
    public string UserName { get; set; } = string.Empty;
    public int? UserId { get; set; }
}

/// <summary>
/// Request model for user creation
/// </summary>
public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}