using System.Collections.Generic;
using MEC;

#if MRP
using Qurre.API.Controllers;
#elif NR
using Qurre.API.World;
#endif

namespace Loli.Addons
{
    static class BroadCasts
    {
        internal static IEnumerator<float> Send()
        {
            Timing.WaitForSeconds(1f);
            for (; ; )
            {
                int random = UnityEngine.Random.Range(1, 100);
#if MRP
                if (random < 20) // 10
                {
                    BroadCast("<size=70%><b><color=#d093ac>🐛 Нашли баг?? 🐜</color></b>\n" +
                              "<color=#0089c7>Вы можете сообщить о нем командой <color=#0089c7>.</color><color=#ff0>баг</color> в консоли на <color=#f47fff>[<color=red>ё</color>]</color></color></size>",
                        20);
                }
                else if (random < 40)
                {
                    // 🎈✨🎭🎨💎
                    BroadCast("<size=70%><b><color=#9d8cc1>🎈 Хотите оставить отзыв?? 💉</color></b>\n" +
                              "<color=#d7bad5>Заходите в наш discord/UCUBU2z\n" +
                              " и оставляйте отзывы на других 🎭 игроков!!</color></size>", 30);
                }
                else if (random < 50)
                {
                    BroadCast(
                        "<size=70%><b><color=#00ffff>У вас <color=red>Жалобы</color>? <color=#15ff00>Предложения</color>? <color=#006dff>Вопросы</color>? " +
                        "<color=#ffb500>Проблемы с сервером</color>?\n<color=#9bff00>Хотите подать заявку на администратора</color>?</color></b>\n" +
                        "<color=#fdffbb>Вы можете открыть тикет в <color=#0089c7>Discord</color>'e</color></size>", 15);
                }
                else if (random < 70)
                {
                    BroadCast("<size=85%><b><color=#ff1f00>🔰</color>" +
                              "<color=#0089c7>Опробуйте кланы - объединяйтесь с другими игроками</color>" +
                              "<color=#ff1f00>🔰</color></b></size>\n" +
                              "<size=70%><color=#bb00ff>Вы можете вступить в клан на сайте <color=#fdffbb>loliscp<color=red>.</color>ru</color></color></size>",
                        20);
                }
                else if (random < 90)
                {
                    BroadCast("<size=85%><b><color=#fff000>✨</color> " +
                              "<color=#48ff00>Не знаете как играть за новые роли?</color>" +
                              " <color=#fff000>✨</color></b></size>\n" +
                              "<size=70%><color=#f47fff>Вы можете посмотреть видео-гайды</color></size>\n" +
                              "<size=70%><color=#ff0033>На youtube канале</color> <color=#ff00ee>fydne</color></size>",
                        20);
                }
                else
                {
                    BroadCast("<size=85%><b><color=#00ffc8>🌎</color> " +
                              "<color=#33ff00>Мы принимаем оплату со всего мира</color>" +
                              " <color=#00ffc8>🌏</color></b></size>\n" +
                              "<size=70%><color=#d91459>Не можете приобрести донат на других серверах?</color></size>\n" +
                              "<size=60%><color=#ff14d8>Мы принимаем платежи со всего мира, Вы сможете приобрести донат даже в Атлантиде</color></size>\n" +
                              "<size=70%><color=#b1ff14>Сайт - <color=#14ffd4>loliscp<color=#1bffec>.</color>ru</color></color></size>",
                        20);
                }
#elif NR
                if (random < 10) // 10
                {
                    BroadCast("<size=70%><b><color=#00ffff>Хотите узнать консольные команды</color>?</b>\n" +
                              "<color=#0089c7>Напишите <color=#0089c7>.</color><color=#ff0>help</color> в консоли на <color=#f47fff>[<color=red>ё</color>]</color></color></size>", 10);
                }
                else if (random < 20) // 20
                {
                    BroadCast("<size=70%><b><color=#00ffff>У вас <color=red>Жалобы</color>? <color=#15ff00>Предложения</color>? <color=#006dff>Вопросы</color>? " +
                              "<color=#ffb500>Проблемы с сервером</color>?\n<color=#9bff00>Хотите подать заявку на администратора</color>?</color></b>\n" +
                              "<color=#fdffbb>Вы можете открыть тикет в <color=#0089c7>Discord</color>'e</color></size>", 15);
                }
                else if (random < 35) // 40
                {
                    BroadCast("<size=85%><b><color=#ff1f00>🔰</color>" +
                              "<color=#0089c7>Опробуйте кланы - объединяйтесь с другими игроками</color>" +
                              "<color=#ff1f00>🔰</color></b></size>\n" +
                              "<size=70%><color=#bb00ff>Вы можете вступить в клан на сайте <color=#fdffbb>loliscp<color=red>.</color>ru</color></color></size>", 20);
                }
                else if (random < 50) // 60
                {
                    BroadCast("<size=85%><b><color=#fff000>💰</color> " +
                              "<color=#fdffbb>Выделитесь среди других игроков, приобретя донат</color>" +
                              " <color=#fff000>💰</color></b></size>\n" +
                              "<size=70%><color=#f47fff>Сделать это Вы можете на на сайте</color> <color=#0089c7>loliscp<color=red>.</color>ru</color></size>", 20);
                }
                else if (random < 65) // 80
                {
                    BroadCast("<b><color=#7aff1b>🔥</color><color=#eb1bff>Хотите разнообразия</color><color=#ff1bf0>?</color><color=#7aff1b>🔥</color></b>\n" +
                              "<size=70%><color=#00ff00>Вы можете <color=#1bffec>кастомизировать</color> своего персонажа на сайте\n" +
                              "<color=#ff00ee>loliscp<color=#1bffec>.</color>ru</color></color></size>", 20);
                }
                else if (random < 80)
                {
                    BroadCast("<size=85%><b><color=#fff000>✨</color> " +
                              "<color=#48ff00>Не знаете как играть за новые роли?</color>" +
                              " <color=#fff000>✨</color></b></size>\n" +
                              "<size=70%><color=#f47fff>Вы можете посмотреть видео-гайды</color></size>\n" +
                              "<size=70%><color=#ff0033>На youtube канале</color> <color=#ff00ee>fydne</color></size>", 20);
                }
                else
                {
                    BroadCast("<size=85%><b><color=#00ffc8>🌎</color> " +
                              "<color=#33ff00>Мы принимаем оплату со всего мира</color>" +
                              " <color=#00ffc8>🌏</color></b></size>\n" +
                              "<size=70%><color=#d91459>Не можете приобрести донат на других серверах?</color></size>\n" +
                              "<size=60%><color=#ff14d8>Мы принимаем платежи со всего мира, Вы сможете приобрести донат даже в Атлантиде</color></size>\n" +
                              "<size=70%><color=#b1ff14>Сайт - <color=#14ffd4>loliscp<color=#1bffec>.</color>ru</color></color></size>", 20);
                }
#endif

                yield return Timing.WaitForSeconds(300f);
            }
        }

        static void BroadCast(string message, ushort duration)
        {
#if MRP
			foreach (var pl in Player.List)
			{
				try
				{
					if (pl.RoleInformation.IsAlive)
						continue;
					pl.Client.Broadcast(message, duration, true);
				}
				catch { }
			}
#elif NR
            Map.Broadcast(message, duration);
#endif
        }
    }
}