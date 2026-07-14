using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NewsMedia.Api.Services;
using NewsMedia.Models;

namespace NewsMedia.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var user = new AppUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Role = "viewer"
            };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);
            return Ok(new { message = "Usuario registrado correctamente" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return Unauthorized(new { message = "Credenciales incorrectas" });

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded) return Unauthorized(new { message = "Credenciales incorrectas" });

            await _signInManager.SignInAsync(user, isPersistent: false);

            var token = _tokenService.GenerateToken(user);

            return Ok(new
            {
                id = user.Id,
                email = user.Email,
                role = user.Role,
                firstName = user.FirstName,
                lastName = user.LastName,
                token
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Sesión cerrada" });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return NotFound();
            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded) return BadRequest(result.Errors);
            return Ok(new { message = "Contraseña actualizada" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return NotFound(new { message = "No existe una cuenta con ese correo." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var tempPassword = "Temp@" + Guid.NewGuid().ToString("N")[..6] + "1";
            var result = await _userManager.ResetPasswordAsync(user, token, tempPassword);

            if (!result.Succeeded)
                return BadRequest(new { message = "No se pudo restablecer la contraseña." });

            return Ok(new { tempPassword, message = "Contraseña restablecida." });
        }
    }

    
    public class RegisterDto
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
    }

    public class LoginDto
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class ChangePasswordDto
    {
        public string Email { get; set; } = "";
        public string CurrentPassword { get; set; } = "";
        public string NewPassword { get; set; } = "";
    }

    public record ResetPasswordDto(string Email);
}