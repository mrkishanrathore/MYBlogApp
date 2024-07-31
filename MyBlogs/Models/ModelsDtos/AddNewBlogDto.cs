namespace MyBlogs.Models.ModelsDtos
{
    public class AddNewBlogDto
    {
        public required Guid UserId { get; set; }
        public required string Image { get; set; }
        public required string ImageData { get; set; }
        public required string Title { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
