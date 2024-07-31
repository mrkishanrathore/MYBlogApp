namespace MyBlogs.Models.ModelsDtos
{
    public class AddCommentDto
    {
        public Guid UserId { get; set; }
        public required Guid BlogId { get; set; }
        public required string CommentText { get; set; }
    }
}
