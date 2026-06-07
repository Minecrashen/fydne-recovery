// Qurre.API.Log → LabApi.Features.Console.Logger
using System;
using LabApi.Features.Console;

namespace Qurre.API
{
    public static class Log
    {
        public static void Info(object msg) => Logger.Info(msg?.ToString() ?? "null");
        public static void Warn(object msg) => Logger.Warn(msg?.ToString() ?? "null");
        public static void Error(object msg) => Logger.Error(msg?.ToString() ?? "null");
        public static void Debug(object msg) => Logger.Debug(msg?.ToString() ?? "null");

        // Qurre: Log.Custom(message, prefix, color)
        public static void Custom(object msg, string prefix = "Loli", ConsoleColor color = ConsoleColor.White)
            => Logger.Raw($"[{prefix}] {msg}", color);
    }
}
