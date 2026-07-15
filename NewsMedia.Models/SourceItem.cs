namespace NewsMedia.Models
{
    public class SourceItem
    {
        public int Id { get; set; }
        public int SourceId { get; set; }
        public string Json { get; set; } = string.Empty;
        public string? SavedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CollectionId { get; set; }

        // Navegación
        public Source? Source { get; set; }
        public Collection? Collection { get; set; }
    }
}