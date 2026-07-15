using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsMedia.Business;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Text.Json;

namespace NewsMedia.Api.Controllers
{
    [ApiController]
    [Route("api/fetch")]
    public class FetchController : ControllerBase
    {
        private readonly ISourceBusiness _sourceBusiness;
        private readonly IHttpClientFactory _httpFactory;

        public FetchController(ISourceBusiness sourceBusiness, IHttpClientFactory httpFactory)
        {
            _sourceBusiness = sourceBusiness;
            _httpFactory = httpFactory;
        }

        [Authorize(Roles = "admin")]
        [HttpGet("{sourceId}")]
        public async Task<IActionResult> FetchSource(int sourceId)
        {
            var source = await _sourceBusiness.GetByIdAsync(sourceId);
            if (source == null) return NotFound();

            var client = _httpFactory.CreateClient();
            if (source.RequiresSecret && !string.IsNullOrEmpty(source.SecretKey))
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {source.SecretKey}");

            try
            {
                if (source.ComponentType == "rss" || source.ComponentType == "xml")
                {
                    var xml = await client.GetStringAsync(source.Url);
                    using var reader = XmlReader.Create(new StringReader(xml));
                    var feed = SyndicationFeed.Load(reader);
                    var items = feed.Items.Take(20).Select(i => new
                    {
                        title = i.Title?.Text,
                        summary = i.Summary?.Text,
                        url = i.Links.FirstOrDefault()?.Uri?.ToString(),
                        publishedAt = i.PublishDate.ToString("o")
                    });
                    return Ok(items);
                }
                else
                {
                    var json = await client.GetStringAsync(source.Url);
                    var parsed = JsonSerializer.Deserialize<object>(json);
                    return Ok(parsed);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}