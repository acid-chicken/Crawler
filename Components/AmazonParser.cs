using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using AngleSharp.Parser.Html;
using Crawler.Models;
using Newtonsoft.Json.Linq;
using static System.Int32;
using static System.String;
using static System.Net.WebUtility;
using static Crawler.Components.Parser;

namespace Crawler.Components
{
    public class AmazonParser : IParser
    {
        private const string sitename = "Amazon";
        private readonly HtmlParser parser;
        private readonly HttpClient client;
        private IEnumerable<string> hostnames = new []
        {
            "www.amazon.com",
            "www.amazon.co.jp",
            "www.amazon.ca",
            "www.amazon.com.br",
            "www.amazon.com.mx",
            "www.amazon.co.uk",
            "www.amazon.de",
            "www.amazon.fr",
            "www.amazon.it",
            "www.amazon.es",
            "www.amazon.nl",
            "www.amazon.cn",
            "www.amazon.in",
            "www.amazon.au"
        };

        public AmazonParser(HtmlParser parser, HttpClient client)
        {
            this.parser = parser;
            this.client = client;
        }

        private bool Test(Uri uri) =>
            hostnames.Any(x => x == uri.Host);

        public bool Test(string url) =>
            Test(new Uri(url));

        public Summaly Parse(string url)
        {
            var html = FetchHtml(parser, client, url);
            var tags = ParseAttributes(html);
            var uri = new Uri(html.Url ?? url);

            string GetTag(params string[] keys) =>
                Parser.GetTag(tags, keys);
            string GetTagFromId(string key) =>
                Parser.GetTagFromId(html, key);
            // string AbsoluteUrl(string path) =>
            //     Parser.AbsoluteUrl(uri, path);
            // string ResolvePath(string path) =>
            //     Parser.ResolvePath(client, uri, path);

            var title = GetTag("title") ??
                GetTagFromId("title") ??
                sitename;
            var description =
                GetTag("description") ??
                GetTagFromId("productDescription");
            var aDynamicImage = HtmlDecode(html.GetElementById("landingImage")?.GetAttribute("data-a-dynamic-image"))?.Trim();
            var thumbnail = IsNullOrEmpty(aDynamicImage) ?
                null :
                JObject.Parse(aDynamicImage).Properties().FirstOrDefault().Name;
            var playerUrl = GetTag("twitter:player");
            var playerWidth = TryParse(
                GetTag("twitter:player:width"),
                out var width) ? width : null as int?;
            var playerHeight = TryParse(
                GetTag("twitter:player:height"),
                out var height) ? height : null as int?;

            return new Summaly(
                sitename,
                title,
                description,
                "https://www.amazon.com/favicon.ico",
                thumbnail,
                playerUrl,
                playerWidth,
                playerHeight);
        }
    }
}
