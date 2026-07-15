using Microsoft.AspNetCore.Mvc;
using NewsMedia.Business;
using NewsMedia.Models;

namespace NewsMedia.Api.Controllers
{
    [ApiController]
    [Route("api/settings")]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingBusiness _business;
        public SettingsController(ISettingBusiness business) => _business = business;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _business.GetAllAsync());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Setting setting)
        {
            var created = await _business.CreateAsync(setting);
            return Ok(created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Setting setting)
        {
            var updated = await _business.UpdateAsync(id, setting);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _business.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}