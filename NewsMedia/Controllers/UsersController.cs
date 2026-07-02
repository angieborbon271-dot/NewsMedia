using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsMedia.Models;

namespace NewsMedia.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        public UsersController(UserManager<AppUser> userManager) => _userManager = userManager;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userManager.Users
                .Select(u => new { u.Id, u.Email, u.FirstName, u.LastName, u.Role })
                .ToListAsync();
            return Ok(users);
        }

        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] string role)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            user.Role = role;
            await _userManager.UpdateAsync(user);
            return Ok(new { message = "Rol actualizado" });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);
            return Ok(new { message = "Usuario eliminado." });
        }
    }

}