using PlayerRoles;
using Qurre.API;
using Qurre.API.Controllers;
using Qurre.API.World;

namespace Loli.Addons
{
    static class Icom
    {
        static internal void Update()
        {
            try
            {
#if NR
                int cdp = 0;
                int rsc = 0;
                int mtf = 0;
                int ci = 0;
                int sh = 0;
                int scp = 0;
                foreach (var pl in Player.List)
                {
                    if (pl.RoleInformation.Role is RoleTypeId.Spectator) continue;
                    switch (pl.RoleInformation.Team)
                    {
                        case Team.ClassD: cdp++; break;
                        case Team.Scientists: rsc++; break;
                        case Team.FoundationForces: mtf++; break;
                        case Team.ChaosInsurgency: ci++; break;
                        case Team.SCPs: scp++; break;
                    }
                }
#endif

#if MRP
				string name = "<size=18%><color=#ff0000>f</color><color=#ff00aa>y</color><color=#ff00d4>d</color><color=#ff00f7>n</color><color=#ea00ff>e</color></size>\n";
				string stats = $"" +
					$"<size=10%>" +
					$"<color=#C0C0C0>Активных генераторов: {Round.ActiveGenerators}</color>\n" +
					$"<color=#00ffff>Длительность раунда: {Round.ElapsedTime.Minutes}:{Round.ElapsedTime.Seconds}</color>" +
					$"</size>";
#elif NR
                string name = "<size=18%><color=#ff0000>f</color><color=#ff00aa>y</color><color=#ff00d4>d</color><color=#ff00f7>n</color><color=#ea00ff>e</color>" +
                              "<color=black> ~~ Промокод на скидку в 5% => 733c-4d12-b261</color></size>\n";
                string stats = $"" +
                               $"<size=12%>Живых:</size>\n" +
                               $"<size=10%>" +
                               $"<color=#ff7100>D: {cdp}</color>\n" +
                               $"<color=#fdffbb>Ученых: {rsc}</color>\n" +
                               $"<color=#0089c7>МОГ: {mtf}</color>\n" +
                               $"<color=#0d9100>ХАОС: {ci}</color>\n" +
                               $"<color=#15ff00>Длани: {sh}</color>\n" +
                               $"<color=#ff0000>SCP: {scp}</color>\n" +
                               $"<color=#C0C0C0>Активных генераторов: {Round.ActiveGenerators}</color>\n" +
                               $"<color=#00ffff>Длительность раунда: {Round.ElapsedTime.Minutes}:{Round.ElapsedTime.Seconds}</color>" +
                               $"</size>";
#endif

                switch (Intercom.Status)
                {
                    case PlayerRoles.Voice.IntercomState.Cooldown:
                        {
                            string newValue = Intercom.RemainingCooldown switch
                            {
                                < 10 => "<color=#ff0>█████████<color=#9c9c00>█</color></color>",
                                < 15 => "<color=#ff0>████████<color=#9c9c00>██</color></color>",
                                < 20 => "<color=#ff0>███████<color=#9c9c00>███</color></color>",
                                < 25 => "<color=#ff0>██████<color=#9c9c00>████</color></color>",
                                < 35 => "<color=#ff0>█████<color=#9c9c00>█████</color></color>",
                                < 40 => "<color=#ff0>████<color=#9c9c00>██████</color></color>",
                                < 50 => "<color=#ff0>███<color=#9c9c00>███████</color></color>",
                                < 55 => "<color=#ff0>██<color=#9c9c00>████████</color></color>",
                                < 60 => "<color=#ff0>█<color=#9c9c00>█████████</color></color>",
                                _ => "<color=#9c9c00>██████████</color>",
                            };
                            Intercom.Text = $"<size=20%>{name}Перезапуск...</size>\n{stats}\n{newValue}";
                            break;
                        }
                    case PlayerRoles.Voice.IntercomState.InUse:
                        {
                            string newValue = Intercom.SpeechRemaining switch
                            {
                                < 10 => "<color=#ff0>█<color=#9c9c00>█████████</color></color>",
                                < 20 => "<color=#ff0>██<color=#9c9c00>████████</color></color>",
                                < 30 => "<color=#ff0>███<color=#9c9c00>███████</color></color>",
                                < 40 => "<color=#ff0>████<color=#9c9c00>██████</color></color>",
                                < 50 => "<color=#ff0>█████<color=#9c9c00>█████</color></color>",
                                < 60 => "<color=#ff0>██████<color=#9c9c00>████</color></color>",
                                < 70 => "<color=#ff0>███████<color=#9c9c00>███</color></color>",
                                < 80 => "<color=#ff0>████████<color=#9c9c00>██</color></color>",
                                < 90 => "<color=#ff0>█████████<color=#9c9c00>█</color></color>",
                                _ => "<color=#ff0>██████████</color>",
                            };
                            Intercom.Text = $"<size=20%>{name}Трансляция...</size>\n{stats}\n{newValue}";
                            break;
                        }
                    default:
                        {
                            Intercom.Text = $"<size=20%>{name}Готово к использованию</size>\n{stats}";
                            break;
                        }
                }
            }
            catch { }
        }
    }
}