using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using AngleSharp.Parser.Html;
using Crawler.Models;
using Newtonsoft.Json.Linq;
using static System.String;
using static Crawler.Components.Parser;

namespace Crawler.Components
{
    public class WikipediaParser : IParser
    {
        private const string sitename = "Wikipedia";
        private readonly HtmlParser parser;
        private readonly HttpClient client;

        public WikipediaParser(HtmlParser parser, HttpClient client)
        {
            this.parser = parser;
            this.client = client;
        }

        public bool Test(string url) =>
            new Uri(url).Host.EndsWith(".wikipedia.org");

        public Summaly Parse(string url)
        {
            var uri = new Uri(url);
            uri = new Uri(uri, $"/w/api.php?format=json&action=query&prop=extracts&exintro=&explaintext=&titles={uri.Segments.LastOrDefault()}");
            url = uri.AbsoluteUri;
            var (success, stream) = Request(parser, client, ref url);
            using (stream)
            if (!success)
                return null;
            else
            using (var reader = new StreamReader(stream))
            {
                var json = ((JObject.Parse(reader.ReadToEnd())
                    .Property("query")?.Value as JObject)?
                    .Property("pages")?.Value as JObject)?
                    .Properties()
                    .FirstOrDefault()?.Value as JObject;

                string GetValue(string key) =>
                    json.Property(key)?.Value.Value<string>();

                var title = Clip(GetValue("title"), 100) ?? sitename;
                var description = Clip(GetValue("extract"), 300);
                var lang = uri.Host.Split('.').Reverse().ElementAtOrDefault(2);
                if (IsNullOrEmpty(lang) || lang == "www")
                    lang = "en";

                return new Summaly(
                    url,
                    sitename,
                    title,
                    description,
                    "https://wikipedia.org/static/favicon/wikipedia.ico",
                    $"https://wikipedia.org/static/images/project-logos/{lang}wiki.png",
                    null,
                    0,
                    0);
            }
        }
    }
}
