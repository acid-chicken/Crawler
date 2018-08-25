using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Microsoft.AspNetCore.Mvc;

namespace Crawler.Controllers
{
    [Route("")]
    [ApiController]
    public class UrlController : ControllerBase
    {
        private readonly HttpClient _client;
        private readonly HtmlParser _parser;

        public UrlController(HtmlParser parser)
        {
            _client = HttpClientFactory.Create();
            _parser = parser;
        }

        private IHtmlDocument Parse(string url) => _parser
            .ParseAsync(_client
                .GetStreamAsync(url)
                .GetAwaiter()
                .GetResult())
            .GetAwaiter()
            .GetResult();

        [HttpGet("[action]")]
        public IActionResult Url(
            [FromQuery] string url) =>
            Content(Parse(url).Head.InnerHtml, "text/html");

        [HttpGet("[action]")]
        public IActionResult Status() =>
            NoContent();
    }
}
