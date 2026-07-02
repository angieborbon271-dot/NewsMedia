using Microsoft.AspNetCore.Mvc;
using NewsMedia.Business;
using NewsMedia.Models;

namespace NewsMedia.Api.Controllers
{
    [ApiController]
    [Route("api/sources")]
    public class SourcesController : ControllerBase
    {
        private readonly SourceBusiness _business;
        public SourcesController(SourceBusiness business) => _business = business;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _business.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var source = await _business.GetByIdAsync(id);
            return source == null ? NotFound() : Ok(source);
        }

        [HttpGet("{id}/items")]
        public async Task<IActionResult> FetchItems(int id, [FromQuery] string? secret)
        {
            var source = await _business.GetByIdAsync(id);
            if (source == null) return NotFound();
            var items = await _business.FetchItemsAsync(source, secret);
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Source source)
        {
            var created = await _business.CreateAsync(source);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Source source)
        {
            var updated = await _business.UpdateAsync(id, source);
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