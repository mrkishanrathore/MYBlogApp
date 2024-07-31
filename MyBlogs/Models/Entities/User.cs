using System.ComponentModel.DataAnnotations;

namespace MyBlogs.Models.Entities
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }

        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public DateTime DateOfCreation { get; set; } = DateTime.Now;

    }
}
