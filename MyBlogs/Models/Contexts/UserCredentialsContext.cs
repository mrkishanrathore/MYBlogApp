using Microsoft.EntityFrameworkCore;
using MyBlogs.Models.Entities;

namespace MyBlogs.Models.Contexts
{
    public class UserCredentialsContext(DbContextOptions<UserCredentialsContext> options) : DbContext(options)
    {
        public DbSet<User> UserCredentials { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogComment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuring primary key
            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);
            
            // Configuring primary key
            modelBuilder.Entity<BlogComment>()
                .HasKey(bc => bc.CommentId);

            // Configuring primary key for Blog
            modelBuilder.Entity<Blog>()
                .HasKey(b => b.BlogId);

            modelBuilder.Entity<Blog>()
                .HasMany(b => b.Comments)
                .WithOne(c => c.Blog)
                .HasForeignKey(c => c.BlogId);

        }
        /*protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customizations and configurations if needed
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("UserCredentials"); // Use the appropriate table name
            });

            modelBuilder.Entity<Blog>(entity =>
            {
                entity.ToTable("Blogs"); // Use the appropriate table name
            });

            modelBuilder.Entity<BlogComment>(entity =>
            {
                entity.ToTable("Comments"); // Use the appropriate table name
            });
        }*/
    }
}
