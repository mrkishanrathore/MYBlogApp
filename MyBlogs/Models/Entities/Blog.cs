using Microsoft.EntityFrameworkCore;

namespace MyBlogs.Models.Entities
{
    public class Blog
    {
        public Guid BlogId { get; set; }
        public required string UserName { get; set; }
        public required Guid UserId { get; set; }
        public required string Image { get; set; }
        public byte[]? ImageData { get; set; }
        public required string Title { get; set; }
        public string Description { get; set; } = string.Empty;

        public ICollection<BlogComment> Comments { get; set; } = new List<BlogComment>();
    }
}
