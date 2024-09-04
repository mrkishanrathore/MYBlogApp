
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBlogs.Models.Contexts;
using MyBlogs.Models.Entities;
using MyBlogs.Models.ModelsDtos;

namespace MyBlogs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogsController(UserCredentialsContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<Blog>>> GetAllBlogs()
        {
            return Ok(await context.Blogs.Include(_ => _.Comments).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<List<Blog>>> GetBlogsOfUser(Guid id)
        {
            return Ok(await context.Blogs.Where(blog => blog.UserId == id).Include(_ => _.Comments).ToListAsync());
        }

        [Route("addBlog")]
        [HttpPost]
        public async Task<ActionResult<Blog>> AddBlog(AddNewBlogDto blog)
        {
            User? thisUser = await context.UserCredentials.FirstOrDefaultAsync(user => user.UserId == blog.UserId);
            if (blog == null || thisUser == null)
            {
                return BadRequest();
            }

            byte[] imageDataBytes = Convert.FromBase64String(blog.ImageData); // Convert base64 string to byte array

            Blog newBlog = new()
            {
                BlogId = Guid.NewGuid(),
                UserName = thisUser.UserName,
                UserId = blog.UserId,
                Image = blog.Image,
                ImageData = imageDataBytes,
                Title = blog.Title,
                Description = blog.Description
            };

            await context.Blogs.AddAsync(newBlog);
            await context.SaveChangesAsync();

            return Ok(newBlog);
        }
        
        [Route("addComment")]
        [HttpPost]
        public async Task<ActionResult> AddCommentInBlog(AddCommentDto comment)
        {
            if(comment != null)
            {
                Blog? blog = await context.Blogs.FirstOrDefaultAsync(blog => blog.BlogId == comment.BlogId);
                User? thisUser = await context.UserCredentials.FirstOrDefaultAsync(user => user.UserId == comment.UserId);
                if (blog != null && thisUser != null)
                {
                    await context.Comments.AddAsync(new BlogComment() { BlogId = comment.BlogId, CommentText = comment.CommentText, UserName = thisUser.UserName, UserId = comment.UserId, CommentId = Guid.NewGuid()});
                    await context.SaveChangesAsync();
                    return Ok("Comment Added");
                }
            }
            return BadRequest();
        }

        [HttpPut]
        public async Task<ActionResult> UpdateBlog(UpdateBlogDto blog)
        {
            if (blog == null)
            {
                return BadRequest();
            }
            Blog? _blog = await context.Blogs.FirstOrDefaultAsync(b => b.BlogId == blog.BlogId);
            if (_blog != null && _blog.UserId == blog.UserId)
            {   
                _blog.ImageData = blog.ImageData?.Length == 0 ? blog.ImageData : _blog.ImageData;
                _blog.Image = string.IsNullOrEmpty(blog.Image)? _blog.Image: blog.Image;
                _blog.Title = string.IsNullOrEmpty(blog.Title) ? _blog.Title : blog.Title;
                _blog.Description = string.IsNullOrEmpty(blog.Description) ? _blog.Description : blog.Description;

                await context.SaveChangesAsync();

                return Ok(blog);
            }
            return BadRequest();
        }

        [HttpDelete]
        public async Task<ActionResult<Blog>> DeleteBlog(DeleteBlogDto id)
        {
            Blog? blog = await context.Blogs.FirstOrDefaultAsync(b => b.BlogId == id.BlogId);
            if (blog == null)
            {
                return BadRequest();
            }
            
            context.Blogs.Remove(blog);

            await context.SaveChangesAsync();
            return Ok(blog);
        }
    }
}
