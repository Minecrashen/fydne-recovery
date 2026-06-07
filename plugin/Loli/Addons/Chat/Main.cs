using Loli.HintsCore;
using Loli.HintsCore.Utils;
using Loli.Webhooks;
using MEC;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Qurre.API.Controllers;
using UnityEngine;
using VoiceChat;

namespace Loli.Addons.Chat;

internal static class Main
{
    internal const string HintBlockTag = "Chat_Main_HintBlock";
    internal const string MutedPlayersHashTag = "Chat_Main_MutedPlayersHash";
    private const string LastSendedMsg = "Chat_LastSendedMsg";

    private static readonly Regex BlockRegex = new("((http|https)+(://)+(?!(([a-zA-Z0-9]+(\\.)loliscp(\\.)ru)|loliscp(\\.)ru)))|(midnight)|(((discord(\\.)gg/)|(discord(\\.)com/invite/))+[a-zA-Z0-9])|(卐)");

    internal static bool GloballyMuted { get; set; } = false;

    [EventMethod(PlayerEvents.Join)]
    private static void Join(JoinEvent ev)
    {
        if (!ev.Player.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
            return;

        DisplayBlock block = new(new Vector2(-3000, -500), new Vector2(700, 1000), new Color32(0, 0, 0, 100), new Padding(100, 30), Align.Left, false);
        display.AddBlock(block);

        ev.Player.Variables[HintBlockTag] = block;
        ev.Player.Variables[MutedPlayersHashTag] = new HashSet<string>();
    }

    internal static void LogChat(Player pl)
    {
        if (!pl.Variables.TryGetAndParse(HintBlockTag, out DisplayBlock block))
            return;

        HashList<MessageBlock> messages = new HashList<MessageBlock>();
        messages.AddRange(block.Contents);
        messages.Reverse();

        foreach (MessageBlock message in messages)
        {
            pl.Client.SendConsole("\n" + message.Content, "white");
        }

        messages.Clear();
    }

    internal static void ClearChat(Player pl)
    {
        if (!pl.Variables.TryGetAndParse(HintBlockTag, out DisplayBlock block))
            return;

        block.Contents.Clear();

        pl.Client.SendConsole($"Чат очищен.", "white");
    }

    internal static void SetVisible(Player pl, bool value)
    {
        if (!pl.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
            return;

        if (!pl.Variables.TryGetAndParse(HintBlockTag, out DisplayBlock block))
            return;

        if (value)
        {
            display.AddBlock(block);
            pl.Client.SendConsole("Вы включили отображение чата.", "white");
        }
        else
        {
            display.RemoveBlock(block);
            pl.Client.SendConsole("Вы скрыли чат.", "white");
        }
    }

    internal static void SetMute(Player pl, string search, bool value)
    {
        Player target = search.GetPlayer();

        if (target is null)
        {
            pl.Client.SendConsole($"Игрок \"{search}\" не найден", "red");
            return;
        }

        if (!target.Variables.TryGetAndParse(MutedPlayersHashTag, out HashSet<string> mutes))
        {
            mutes = new HashSet<string>();
            target.Variables[MutedPlayersHashTag] = mutes;
        }

        if (value)
        {
            mutes.Add(target.UserInformation.UserId);
            pl.Client.SendConsole(
                $"Игрок \"{search}\" успешно замучен. Чтобы удалить сообщения от него, вы можете почистить чат.",
                "white");
        }
        else
        {
            mutes.Remove(target.UserInformation.UserId);
            pl.Client.SendConsole($"Игрок \"{search}\" успешно размучен.", "white");
        }
    }

    internal static void SendMessage(Player author, MessageType type, string[] args)
    {
        if (VoiceChatMutes.Mutes.Contains(author.UserInformation.UserId) ||
            VoiceChatMutes.GetFlags(author.ReferenceHub).HasFlag(VcMuteFlags.GlobalRegular))
        {
            author.Client.SendConsole("Вы глобально замучены.", "red");
            return;
        }

        if (GloballyMuted)
        {
            author.Client.SendConsole("Чат глобально замучен.", "red");
            return;
        }

        string content = string.Join(" ", args);

        if (BlockRegex.IsMatch(content))
        {
            author.Client.SendConsole("Ваше сообщение содержит недопустимые символы.", "red");
            return;
        }

        if (content.Length > 250)
        {
            author.Client.SendConsole("Максимальное число символов - 250", "red");
            return;
        }

        if (content.Length == 0)
        {
            author.Client.SendConsole("Вы не можете написать пустое сообщение", "red");
            return;
        }

        if (type is MessageType.Public && !author.RoleInformation.IsAlive)
        {
            author.Client.SendConsole("Мертвые не могут писать в публичный чат :(", "red");
            return;
        }

        if (author.Variables.TryGetAndParse(LastSendedMsg, out DateTime lastSended))
        {
            double rateLimit = (DateTime.Now - lastSended).TotalSeconds;
            if (rateLimit < 5)
            {
                author.Client.SendConsole(
                    $"Сообщения можно отправлять раз в 5 секунд. Подождите еще {(int)rateLimit} секунд.", "red");
                return;
            }
        }

        author.Variables[LastSendedMsg] = DateTime.Now;

        AuthorLite authorLite = new(author);
        Message message = new(author, type, content);
        MessageBlock msgBlock = new(message.RawText, new Color32(255, 112, 115, 255), "60%");
        HashSet<DisplayBlock> deleteLater = new();

        Func<Player, bool> act = type switch
        {
            MessageType.Position or MessageType.Nonrp => pl => Vector3.Distance(pl.MovementState.Position, authorLite.Position) < 10,
            MessageType.Public => _ => true,
            MessageType.Ally => pl => pl.RoleInformation.Faction == authorLite.Faction,
            MessageType.Team => pl => pl.RoleInformation.Team == authorLite.Team,
            MessageType.Clan => pl => pl.GetClan() == authorLite.Clan,
            MessageType.Admin => pl => pl == author || pl.ItsAdmin(),
            _ => _ => false,
        };

        foreach (Player pl in Player.List)
        {
            if (!act(pl))
                continue;

            if (!pl.Variables.TryGetAndParse(HintBlockTag, out DisplayBlock block))
                continue;

            if (pl.Variables.TryGetAndParse(MutedPlayersHashTag, out HashSet<string> mutes) &&
                mutes.Contains(authorLite.UserId))
                continue;

            block.Contents.Insert(0, msgBlock);
            deleteLater.Add(block);

            pl.Client.SendConsole("\n" + message.RawText, "white");
        }

        Timing.CallDelayed(60f, () =>
        {
            foreach (DisplayBlock block in deleteLater)
                block.Contents.Remove(msgBlock);

            deleteLater.Clear();
        });
    }

    internal static void SendPrivateMessage(Player author, string search, string[] args)
    {
        if (VoiceChatMutes.Mutes.Contains(author.UserInformation.UserId) ||
            VoiceChatMutes.GetFlags(author.ReferenceHub).HasFlag(VcMuteFlags.GlobalRegular))
        {
            author.Client.SendConsole("Вы глобально замучены.", "red");
            return;
        }

        string content = string.Join(" ", args);

        if (BlockRegex.IsMatch(content))
        {
            author.Client.SendConsole("Ваше сообщение содержит недопустимые символы.", "red");
            return;
        }

        if (content.Length > 250)
        {
            author.Client.SendConsole("Максимальное число символов - 250", "red");
            return;
        }

        if (content.Length == 0)
        {
            author.Client.SendConsole("Вы не можете написать пустое сообщение", "red");
            return;
        }

        Player target = search.GetPlayer();

        if (target is null)
        {
            author.Client.SendConsole($"Игрок \"{search}\" не найден", "red");
            return;
        }

        if (target == author)
        {
            author.Client.SendConsole("Нельзя писать самому себе", "red");
            return;
        }

        Message message = new Message(author, MessageType.Private, content);
        MessageBlock msgBlock = new MessageBlock(message.RawText, new Color32(255, 112, 115, 255), "60%");

        if (!target.Variables.TryGetAndParse(HintBlockTag, out DisplayBlock block))
            return;

        if (!author.Variables.TryGetAndParse(HintBlockTag, out DisplayBlock blockAuthor))
            return;

        if (target.Variables.TryGetAndParse(MutedPlayersHashTag, out HashSet<string> mutes) &&
            mutes.Contains(author.UserInformation.UserId))
            return;

        block.Contents.Insert(0, msgBlock);
        blockAuthor.Contents.Insert(0, msgBlock);

        target.Client.SendConsole("\n" + message.RawText, "white");
        author.Client.SendConsole("\n" + message.RawText, "white");

        Timing.CallDelayed(60f, () =>
        {
            block.Contents.Remove(msgBlock);
            blockAuthor.Contents.Remove(msgBlock);
        });
    }

    internal static void WebhookSend(Message message)
    {
        if (message.Type is MessageType.Private)
            return;

        Dishook hook = new Dishook(Core.WebHooks.Chat);
        Embed embed = new Embed
        {
            Author = new EmbedAuthor
            {
                Name = $"{message.Author.UserInformation.Nickname} - {message.Author.UserInformation.UserId}"
            },
            Footer = new EmbedFooter
            {
                Text = $"{message.Role}"
            },
            Description = $"{Message.GetChatString(message.Type).Item1}\n```{message.Text}```",
            TimeStamp = message.Date,
            Color = 3866881
        };

        hook.Send(string.Empty, Core.ServerName, embeds: [embed]);
    }
}