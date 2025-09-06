using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GearHawk.API.Controllers
{
    public class LoginRequest
    {
        [Required]
        public string Uid { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string DisplayName { get; set; } = "";
    }

    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            var claims = User?.Claims.Select(c => new { type = c.Type, value = c.Value }) ?? Enumerable.Empty<object>();
            return Ok(new { success = true, claims });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                // Your login logic here
                return Ok(new
                {
                    success = true,
                    data = new { uid = request.Uid } // Example response
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}