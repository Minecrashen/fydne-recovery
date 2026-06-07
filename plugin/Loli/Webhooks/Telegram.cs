using System;
using System.Net.Http.Headers;
using System.Net.Http;

namespace Loli.Webhooks
{
    static class Telegram
    {
        internal static void Send(string text)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.telegram.org/bot6110931633%3AAAEu4ILOHzL_dg-YjTawTOp9Ry9S_Sl3Bq4/sendMessage"),
                Headers =
                {
                    { "accept", "application/json" },
                    { "User-Agent", "Telegram Bot SDK - (https://github.com/irazasyed/telegram-bot-sdk)" },
                },
                Content = new StringContent("{\"text\":\"" + text + "\",\"disable_web_page_preview\":false,\"disable_notification\":false,\"reply_to_message_id\":null,\"chat_id\":\"-934074586\"}")
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