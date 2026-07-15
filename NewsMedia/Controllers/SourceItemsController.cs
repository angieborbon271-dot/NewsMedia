using Microsoft.AspNetCore.Mvc;
using NewsMedia.Business;
using NewsMedia.Models;

namespace NewsMedia.Api.Controllers
{
    [ApiController]
    [Route("api/source-items")]
    public class SourceItemsController : ControllerBase
    {
        private readonly ISourceItemBusiness _business;
        public SourceItemsController(ISourceItemBusiness business) => _business = business;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _business.GetAllAsync());

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(string userId) =>
            Ok(await _business.GetByUserAsync(userId));

        [HttpGet("source/{sourceId}")]
        public async Task<IActionResult> GetBySource(int sourceId) =>
            Ok(await _business.GetBySourceAsync(sourceId));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SourceItem item)
        {
            var created = await _business.CreateAsync(item);
            return Ok(created);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _business.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpGet("export/{userId}")]
        public async Task<IActionResult> Export(string userId)
        {
            var json = await _business.ExportToJsonAsync(userId);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            return File(bytes, "application/json", "mis-noticias.json");
        }

        [HttpPost("import/{userId}")]
        public async Task<IActionResult> Import(string userId, IFormFile file)
        {
            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();
            var count = await _business.ImportFromJsonAsync(content, userId);
            return Ok(new { imported = count });
        }
    }
}