using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MyBlogs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestartController : ControllerBase
    {
        private readonly IRestartService _restartService;

        public RestartController(IRestartService restartService)
        {
            _restartService = restartService;
        }

        [HttpPost()]
        public async Task<IActionResult> RestartApplication()
        {
            Console.WriteLine("Restart");
            await _restartService.RestartApplicationAsync();
            return Ok("Application is restarting...");
        }
    }
}
