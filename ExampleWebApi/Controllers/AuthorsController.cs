using ExampleWebApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExampleWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public AuthorsController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Author author)
        {
            await context.Authors.ExecutePutAsync(author);
            return Ok();
        }

        [HttpGet] 
        public async Task<IActionResult> Get(string id) 
        {
            return this.Ok(await context.Authors.FirstOrDefaultAsync(id));
        }

        [HttpGet("List")]
        public async Task<IActionResult> GetList(string? token)
        {
            return this.Ok(await context.Authors.ScannedListAsync(10, token));
        }
    }
}
