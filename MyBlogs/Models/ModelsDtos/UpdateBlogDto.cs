namespace MyBlogs.Models.ModelsDtos
{
    public class UpdateBlogDto
    {
        public required Guid BlogId { get; set; }
        public required Guid UserId { get; set; }
        public string? Image { get; set; }
        public byte[]? ImageData { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; } = string.Empty;
    }
}
