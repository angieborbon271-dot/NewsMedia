namespace NewsMedia.Models
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ComponentType { get; set; } = "rss";
        public string? Description { get; set; }
        public bool RequiresSecret { get; set; } = false;
        public string? SecretKey { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relación: una fuente tiene muchos items
        public ICollection<SourceItem> SourceItems { get; set; } = new List<SourceItem>();
    }
}