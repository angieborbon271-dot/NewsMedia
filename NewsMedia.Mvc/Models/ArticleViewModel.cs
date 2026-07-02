namespace NewsMedia.Mvc.Models;

public class ArticleViewModel
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Url { get; set; }
    public string UrlToImage { get; set; }
    public string PublishedAt { get; set; }
    public SourceViewModel Source { get; set; }
}

public class SourceViewModel
{
    public string Id { get; set; }
    public string Name { get; set; }
}

public class NewsResponseViewModel
{
    public string Status { get; set; }
    public int TotalResults { get; set; }
    public List<ArticleViewModel> Articles { get; set; }
}