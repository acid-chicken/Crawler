
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Crawler.Models;
using static System.String;

namespace Crawler.Components
{
    public class Parser
    {
        private readonly HtmlParser parser;
        private readonly HttpClient client;
        private readonly IEnumerable<IParser> parsers;
        private static readonly IEnumerable<string> separators = new []
        {
            "-",
            "|",
            ":",
            "・"
        };

        public Parser(HtmlParser parser, IHttpClientFactory factory)
        {
            this.client = factory.CreateClient("crawler");
            this.parser = parser;
            this.parsers = new []
            {
                new AmazonParser(parser, client),
                new WikipediaParser(parser, client),
                new DefaultParser(parser, client) as IParser
            };
        }

        internal static (bool success, Stream stream) Request(HtmlParser parser, HttpClient client, ref string url)
        {
            var response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
                return (true, response.Content.ReadAsStreamAsync().Result);
            else
            {
                var status = (int)response.StatusCode;
                if (300 <= status && status <= 399)
                {
                    url = response.Headers.Location.AbsoluteUri;
                    return Request(parser, client, ref url);
                }
                return (false, null);
            }
        }

        internal static (bool success, IHtmlDocument html) FetchHtml(HtmlParser parser, HttpClient client, ref string url)
        {
            var (success, stream) = Request(parser, client, ref url);
            return (success, success ? parser.Parse(stream) : null);
        }

        internal static IDictionary<string, string> ParseAttributes(IHtmlDocument html) =>
            html.GetElementsByTagName("meta")
                    .Select(meta =>
                        KeyValuePair.Create(
                            meta.Attributes.FirstOrDefault(x => x.Name == "name") ??
                            meta.Attributes.FirstOrDefault(x => x.Name == "property"),
                            meta.Attributes.FirstOrDefault(x => x.Name == "content")))
                .Concat(html.GetElementsByTagName("link")
                    .Select(link =>
                        KeyValuePair.Create(
                            link.Attributes.FirstOrDefault(x => x.Name == "rel"),
                            link.Attributes.FirstOrDefault(x => x.Name == "href"))))
                .Where(x =>
                    !IsNullOrWhiteSpace(x.Key?.Value) &&
                    !IsNullOrWhiteSpace(x.Value?.Value))
                .Aggregate(
                    new Dictionary<string, string>(),
                    (a, c) =>
                    {
                        a.TryAdd(c.Key.Value, c.Value.Value);
                        return a;
                    });

        internal static string GetTag(IDictionary<string, string> tags, params string[] keys)
        {
            var buffer = null as string;
            return keys.FirstOrDefault(x => tags.TryGetValue(x, out buffer)) is null ?
                null :
                buffer.Trim();
        }

        private static IEnumerable<string> FlattenHtml(IEnumerable<INode> nodes) =>
            nodes.SelectMany(node =>
            {
                switch (node)
                {
                    case IText text: return new [] { text.Data?.Trim() };
                    case IElement element: return FlattenHtml(node.ChildNodes);
                    default: return new string[0];
                }
            });

        internal static string GetTagFromId(IHtmlDocument html, string id) =>
            FlattenHtml(html.GetElementById(id)?.ChildNodes)
            .Aggregate(
                new StringBuilder(),
                (a, c) => a.Append(c))
            .ToString();

        internal static string AbsoluteUrl(Uri uri, string path)
        {
            var pathUri = path is null ? null : new Uri(path);
            return pathUri is null ? null :
                (pathUri.IsAbsoluteUri ? pathUri : new Uri(uri, pathUri)).AbsoluteUri;
        }
        internal static string ResolvePath(HttpClient client, Uri uri, string path)
        {
            string TryGetUrl(Uri location)
            {
                using (var response = client.GetAsync(location).Result)
                {
                    if (response.IsSuccessStatusCode)
                        return response.RequestMessage.RequestUri.AbsoluteUri;
                    else
                    {
                        var status = (int)response.StatusCode;
                        if (300 <= status && status <= 399)
                        {
                            return TryGetUrl(response.Headers.Location);
                        }
                    }
                }
                var next = new Uri(
                    new Uri(uri.GetLeftPart(UriPartial.Authority)),
                    location);
                return next.AbsoluteUri == location.AbsoluteUri ? null : TryGetUrl(next);
            }

            return TryGetUrl(new Uri(uri, path));
        }

        internal static string Clip(string source, int length) =>
            (source?.Length ?? 0) > length ? $"{source.Substring(0, length)}..." : source;

        internal static string CleanTitle(string source, string target)
        {
            var separator = separators.FirstOrDefault(x => source.EndsWith($" {x} {target}"));
            return separator is null ?
                source :
                source.Remove(source.Length - target.Length + separator.Length + 2);
        }

        internal static string OrDefault(string source, string fallback) =>
                IsNullOrWhiteSpace(source) ? fallback : source;

        public Summaly Parse(string url) =>
            parsers.FirstOrDefault(x => x.Test(url))?.Parse(url);
    }
}
