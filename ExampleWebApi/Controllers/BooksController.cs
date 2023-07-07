using ExampleWebApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExampleWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public BooksController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string? token)
        {
            return this.Ok(await context.Books.ScannedListAsync(10, token));
        }

        [HttpGet("WithoutLink")]
        public async Task<IActionResult> GetWithoutLink(string id)
        {
            return this.Ok(await context.Books.FirstOrDefaultAsync(id));
        }

        [HttpGet("WithLink")]
        public async Task<IActionResult> GetWithLink(string id)
        {
            return this.Ok(await context.Books.FirstOrDefaultLinkedAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> Create(string isbn, string name, string authorId)
        {
            Book book = new Book
            {
                ISBN = isbn,
                BookName = name, 
                AuthorId = authorId
            };

            await context.Books.ExecutePutAsync(book);
            return Ok();
        }
    }
}
