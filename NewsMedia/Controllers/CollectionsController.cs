using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsMedia.Business;
using NewsMedia.Models;
using System.Security.Claims;

namespace NewsMedia.Api.Controllers
{
	[Authorize]
	[ApiController]
	[Route("api/collections")]
	public class CollectionsController : ControllerBase
	{
		private readonly ICollectionBusiness _business;
		public CollectionsController(ICollectionBusiness business) => _business = business;

		private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

		// Lista las carpetas del usuario logueado
		[HttpGet]
		public async Task<IActionResult> GetMine()
		{
			var userId = CurrentUserId;
			if (userId == null) return Unauthorized();

			var collections = await _business.GetByOwnerAsync(userId);
			return Ok(collections);
		}

		// Detalle de una carpeta (con sus items) — solo si es dueño
		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(int id)
		{
			var userId = CurrentUserId;
			if (userId == null) return Unauthorized();

			var collection = await _business.GetByIdAsync(id, userId);
			return collection == null ? NotFound() : Ok(collection);
		}

		// Crea una carpeta nueva para el usuario logueado
		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreateCollectionDto dto)
		{
			var userId = CurrentUserId;
			if (userId == null) return Unauthorized();

			var collection = new Collection
			{
				Name = dto.Name,
				Description = dto.Description,
				OwnerId = userId,
				CreatedAt = DateTime.UtcNow
			};

			var created = await _business.CreateAsync(collection);
			return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
		}

		// Borra una carpeta (solo si es dueño)
		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var userId = CurrentUserId;
			if (userId == null) return Unauthorized();

			var deleted = await _business.DeleteAsync(id, userId);
			return deleted ? NoContent() : NotFound();
		}

		// Asigna una noticia guardada a una carpeta
		[HttpPost("{id}/items/{sourceItemId}")]
		public async Task<IActionResult> AssignItem(int id, int sourceItemId)
		{
			var userId = CurrentUserId;
			if (userId == null) return Unauthorized();

			var ok = await _business.AssignItemAsync(id, sourceItemId, userId);
			return ok ? Ok(new { message = "Noticia asignada a la carpeta." }) : NotFound();
		}

		// Quita una noticia de la carpeta en la que esté (vuelve a quedar "sin carpeta")
		[HttpDelete("items/{sourceItemId}")]
		public async Task<IActionResult> RemoveItem(int sourceItemId)
		{
			var userId = CurrentUserId;
			if (userId == null) return Unauthorized();

			var ok = await _business.RemoveItemAsync(sourceItemId, userId);
			return ok ? Ok(new { message = "Noticia quitada de la carpeta." }) : NotFound();
		}
	}

	public class CreateCollectionDto
	{
		public string Name { get; set; } = "";
		public string? Description { get; set; }
	}
}