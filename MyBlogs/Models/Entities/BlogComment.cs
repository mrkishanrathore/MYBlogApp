namespace MyBlogs.Models.Entities
{
    public class BlogComment
    {
        public Guid CommentId { get; set; }
        public Guid UserId { get; set; }
        public required Guid BlogId { get; set; }
        public required string CommentText { get; set; }
        public required string UserName { get; set; }

        public Blog Blog { get; set; }
    }
}
