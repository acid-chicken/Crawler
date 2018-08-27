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
        public IActionResult GetUrl(
            [FromQuery] string url)
        {
            var summaly = parser.Parse(url);
            return summaly is null ?
                BadRequest() :
                Ok(summaly) as IActionResult;
        }

        [HttpGet("[action]")]
        public IActionResult Status() =>
            NoContent();
    }
}
