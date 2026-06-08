using System;
using System.Net.Http.Headers;
using System.Net.Http;

namespace Loli.Webhooks
{
    static class Telegram
    {
        internal static void Send(string text)
        {
            string token = Environment.GetEnvironmentVariable("FYDNE_TELEGRAM_BOT_TOKEN") ?? string.Empty;
            string chatId = Environment.GetEnvironmentVariable("FYDNE_TELEGRAM_CHAT_ID") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(chatId))
                return;

            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"https://api.telegram.org/bot{token}/sendMessage"),
                Headers =
                {
                    { "accept", "application/json" },
                    { "User-Agent", "Telegram Bot SDK - (https://github.com/irazasyed/telegram-bot-sdk)" },
                },
                Content = new StringContent("{\"text\":\"" + text + "\",\"disable_web_page_preview\":false,\"disable_notification\":false,\"reply_to_message_id\":null,\"chat_id\":\"" + chatId + "\"}")
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };
            client.SendAsync(request).Start();
        }
    }
}
