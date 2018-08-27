using Newtonsoft.Json;

namespace Crawler.Models
{
    [JsonObject(MemberSerialization.Fields)]
    public class Summaly
    {
        private string url;
        private string sitename;
        private string title;
        private string description;
        private string icon;
        private string thumbnail;
        private Player player;

        public Summaly()
        {
        }

        public Summaly(
            string url,
            string sitename,
            string title,
            string description,
            string icon,
            string thumbnail,
            string playerUrl,
            int? playerWidth,
            int? playerHeight)
        {
            this.url = url;
            this.sitename = sitename;
            this.title = title;
            this.description = description;
            this.icon = icon;
            this.thumbnail = thumbnail;
            this.player = new Player(
                playerUrl,
                playerWidth,
                playerHeight);
        }
    }

    [JsonObject(MemberSerialization.Fields)]
    public class Player
    {
        private string url;

        private int? width;

        private int? height;

        public Player()
        {
        }

        public Player(
            string url,
            int? width,
            int? height)
        {
            this.url = url;
            this.width = width;
            this.height = height;
        }
    }
}
