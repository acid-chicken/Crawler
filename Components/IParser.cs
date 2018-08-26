using Crawler.Models;

namespace Crawler.Components
{
    public interface IParser
    {
        bool Test(string url);
        Summaly Parse(string url);
    }
}
