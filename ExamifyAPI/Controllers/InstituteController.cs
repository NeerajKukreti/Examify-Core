using ExamAPI.Services;
using ExamifyAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model.DTO;

namespace ExamifyAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InstituteController : ControllerBase
    {
        private readonly IInstituteService _instituteService;
        private readonly IAuthService _authService;

        public InstituteController(IInstituteService instituteService, IAuthService authService)
        {
            _instituteService = instituteService;
            _authService = authService;
        }

        // GET: api/institute/list
        [HttpGet("list")]
        public async Task<IActionResult> GetAllInstitutes()
        {
            var institutes = await _instituteService.GetAllInstitutesAsync();
            return Ok(new
            {
                Success = true,
                Count = institutes?.Count() ?? 0,
                Data = institutes
            });
        }

        // GET: api/institute/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetInstituteById(int id)
        {
            var institute = await _instituteService.GetInstituteByIdAsync(id);
            if (institute == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = $"Institute with Id {id} not found."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Institute retrieved successfully.",
                Data = institute
            });
        }

        // POST: api/institute
        // Body must include Password (for new institute login user)
        [HttpPost]
        public async Task<IActionResult> CreateInstitute([FromBody] InstituteDTO dto)
        {
            if (dto == null)
                return BadRequest(new { Success = false, Message = "Request body cannot be empty." });

            if (!ModelState.IsValid)
                return BadRequest(new { Success = false, Message = "Validation failed.", Errors = ModelState });

            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { Success = false, Message = "Password is required when creating a new institute." });

            var createdBy = _authService.GetCurrentUserID();
            var newId = await _instituteService.InsertOrUpdateInstituteAsync(dto, null, createdBy, null);

            if (newId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Failed to create institute. Please try again." });
            }

            return CreatedAtAction(nameof(GetInstituteById), new { id = newId },
                new
                {
                    Success = true,
                    InstituteId = newId,
                    Message = "Institute created successfully along with a login user."
                });
        }

        // PUT: api/institute/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateInstitute(int id, [FromBody] InstituteDTO dto)
        {
            if (dto == null)
                return BadRequest(new { Success = false, Message = "Request body cannot be empty." });

            if (!ModelState.IsValid)
                return BadRequest(new { Success = false, Message = "Validation failed.", Errors = ModelState });

            if (dto.UserId <= 0)
                return BadRequest(new { Success = false, Message = "UserId is required when updating an institute." });

            var modifiedBy = _authService.GetCurrentUserID();
            var updatedId = await _instituteService.InsertOrUpdateInstituteAsync(dto, id, null, modifiedBy);

            if (updatedId <= 0)
            {
                return BadRequest(new { Success = false, Message = $"Failed to update institute with Id {id}." });
            }

            return Ok(new
            {
                Success = true,
                InstituteId = updatedId,
                Message = "Institute updated successfully."
            });
        }
    }
}
