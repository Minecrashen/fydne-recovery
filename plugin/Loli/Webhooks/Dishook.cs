using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace Loli.Webhooks
{
    [JsonObject]
    public class Dishook
    {
        private readonly HttpClient _httpClient;
        private readonly string _webhookUrl;

        [JsonProperty("content")] public string Content { get; set; }
        [JsonProperty("username")] public string Username { get; set; }
        [JsonProperty("avatar_url")] public string AvatarUrl { get; set; }
        [JsonProperty("tts")] public bool IsTTS { get; set; }
        [JsonProperty("embeds")] public List<Embed> Embeds { get; set; } = new();

        public Dishook(string webhookUrl)
        {
            HttpClientHandler handler = new()
            {
                PreAuthenticate = true,
                UseDefaultCredentials = false
            };

            _httpClient = new HttpClient(handler);
            _webhookUrl = webhookUrl.Replace("discord.com", "discord.loli-xxx.baby");
        }

        public Dishook(ulong id, string token) : this($"https://discord.com/api/webhooks/{id}/{token}")
        {
        }

        public void Send(string content, string username = null, string avatarUrl = null, bool isTTS = false,
            IEnumerable<Embed> embeds = null)
        {
            Content = content;
            Username = username;
            AvatarUrl = avatarUrl;
            IsTTS = isTTS;
            Embeds.Clear();
            if (embeds is not null)
                Embeds.AddRange(embeds);

            StringContent contentData = new(JsonConvert.SerializeObject(this), Encoding.UTF8, "application/json");

            _httpClient.PostAsync(_webhookUrl, contentData);
        }
    }
}