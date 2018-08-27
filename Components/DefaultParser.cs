using System;
using System.Net.Http;
using AngleSharp.Parser.Html;
using Crawler.Models;
using static System.Int32;
using static System.Net.WebUtility;
using static Crawler.Components.Parser;

namespace Crawler.Components
{
    public class DefaultParser : IParser
    {
        private readonly HtmlParser parser;
        private readonly HttpClient client;

        public DefaultParser(HtmlParser parser, HttpClient client)
        {
            this.parser = parser;
            this.client = client;
        }

        public bool Test(string url) =>
            true;

        public Summaly Parse(string url)
        {
            var (success, html) = FetchHtml(parser, client, ref url);
            if (!success)
                return null;
            var tags = ParseAttributes(html);
            var uri = new Uri(url);

            string GetTag(params string[] keys) =>
                Parser.GetTag(tags, keys);
            string AbsoluteUrl(string path) =>
                Parser.AbsoluteUrl(uri, path);
            string ResolvePath(string path) =>
                Parser.ResolvePath(client, uri, path);

            var sitename = HtmlDecode(OrDefault(
                GetTag("og:site_name", "application-name"),
                uri.Host));
            var title = Clip(
                OrDefault(
                    CleanTitle(
                        HtmlDecode(OrDefault(
                            GetTag("og:title", "twitter:title", "title"),
                            html.Title)),
                        sitename),
                    sitename),
                100);
            var description = Clip(
                HtmlDecode(GetTag("og:description", "twitter:description", "description")),
                300);
            var icon = ResolvePath(OrDefault(
                GetTag("shortcut icon", "icon"),
                "/favicon.ico"));
            var thumbnail = AbsoluteUrl(GetTag("og:image", "twitter:image", "image_src", "apple-touch-icon", "apple-touch-icon image_src"));
            var playerUrl = GetTag("twitter:player");
            var playerWidth = TryParse(
                GetTag("twitter:player:width"),
                out var width) ? width : null as int?;
            var playerHeight = TryParse(
                GetTag("twitter:player:height"),
                out var height) ? height : null as int?;
            return new Summaly(
                url,
                sitename,
                title,
                description,
                icon,
                thumbnail,
                playerUrl,
                playerWidth,
                playerHeight);
        }
    }
}
