using Crawler.Components;
using Crawler.Models;
using Microsoft.AspNetCore.Mvc;

namespace Crawler.Controllers
{
    [Route("")]
    [ApiController]
    public class UrlController : ControllerBase
    {
        private Parser parser;

        public UrlController(Parser parser)
        {
            this.parser = parser;
        }

        [HttpGet("url")]
        public Summaly GetUrl(
            [FromQuery] string url) =>
            parser.Parse(url);

        [HttpGet("[action]")]
        public IActionResult Status() =>
            NoContent();
    }
}
