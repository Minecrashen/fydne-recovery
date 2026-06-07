using Loli.Concepts.Scp008;
using PlayerRoles;
using Qurre.API;
using System;
using Qurre.API.Controllers;
using UnityEngine;

namespace Loli.Addons.Chat;

internal class Message
{
    internal DateTime Date { get; }
    internal Player Author { get; }
    internal MessageType Type { get; }
    internal string Text { get; }
    internal RoleTypeId Role { get; }
    internal Vector3 Position { get; }

    internal string RawText { get; }

    internal Message(Player author, MessageType type, string message)
    {
        Date = DateTime.Now;
        Author = author;
        Type = type;
        Role = author.RoleInformation.Role;
        Position = author.MovementState.Position;

        Text = message
            .Replace("<", "")
            .Replace(">", "")
            .Replace("\u003c", "")
            .Replace("\u003e", "")
            .Replace("\u003C", "")
            .Replace("\u003E", "")
            .Replace("\\u", "/u")
            .Trim();

        (string, string) roleInfo = Role.GetInfoRole();
        (string, string) messageInfo = GetChatString(Type);

        if (Author.ItsHacker())
            roleInfo.Item1 = "Хакер Повстанцев";
        else if (Author.Tag.Contains(SerpentsHand.Tag))
            roleInfo.Item1 = "Длань Змея";

        RawText = string.Empty;
        RawText += $"<color={roleInfo.Item2}>{Author.UserInformation.Nickname.OptimizeNick(15)}</color>";
        RawText += " • ";
        RawText += $"<color={messageInfo.Item2}>" +
            $"{Date:mm:ss} | {messageInfo.Item1}" +
            $"</color>";
        RawText += " • ";
        RawText += $"<color={roleInfo.Item2}>{roleInfo.Item1}</color>";
        RawText += "\n";

        if (author.IsPrime())
        {
            try
            {
                RawText += GradientColorTag.GradientText(
                    Text, new Color32(212, 175, 55, 255), new Color32(240, 110, 255, 255) // #d4af37 -> #f06eff
                );
            }
            catch
            {
                RawText += $"<color=#d4af37>{Text}</color>";
            }
        }
        else
            RawText += Text;

        Main.WebhookSend(this);
    }

    static internal (string, string) GetChatString(MessageType type)
    {
        return type switch
        {
            MessageType.Position => ("Ближний", "#daffbb"),
            MessageType.Public => ("Публичный", "#c671da"),
            MessageType.Ally => ("Союзный", "#39dcdf"),
            MessageType.Team => ("Командный", "#4865fc"),
            MessageType.Private => ("Личный", "#ffc600"),
            MessageType.Clan => ("Клан", "#db5bae"),
            MessageType.Admin => ("Админ", "#a60000"),
            MessageType.Nonrp => ("NonRP", "#6e6e6e"),
            _ => ("Неизвестно", "#6e6e6e")
        };
    }
}