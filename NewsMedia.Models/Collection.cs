namespace NewsMedia.Models
{
    public class Collection
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string OwnerId { get; set; } = string.Empty;   // Id de AspNetUsers
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<SourceItem> Items { get; set; } = new List<SourceItem>();
    }
}
