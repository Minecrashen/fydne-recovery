using Loli.Addons;
#if MRP
using Loli.Addons.RolePlay;
#endif
using Loli.Builds.Models.Rooms;
using Loli.Concepts;
using Loli.Concepts.Hackers;
using Loli.Concepts.Scp008;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Linq;
using Qurre.API.Controllers;
using Qurre.API.World;

namespace Loli.Modules
{
    static class RoundCheck
    {
        [EventMethod(RoundEvents.Check)]
        static void Check(RoundCheckEvent ev)
        {
            try
            {
                ev.End = false;

                RoundSummary.SumInfo_ClassList list = default;
                int sh = 0;
                bool checkAlive = Round.ElapsedTime.TotalMinutes > 5 && Player.List.Count() > 5;
                foreach (var pl in Player.List)
                {
                    try
                    {
                        if (Range.InvisibleInCheck(pl))
                            continue;

                        if (checkAlive && !Caches.IsAlive(pl.UserInformation.UserId))
                            continue;

#if MRP
                        if (pl.Tag.Contains(FacilityManager.TagSpy))
                            continue;
#endif

                        if (pl.InPocket())
                            continue;

                        if (pl.Tag.Contains(SerpentsHand.Tag))
                            sh++;
                        else
                            switch (pl.RoleInformation.Team)
                            {
                                case Team.ClassD:
                                    list.class_ds++;
                                    break;
                                case Team.Scientists:
                                    list.scientists++;
                                    break;
                                case Team.ChaosInsurgency:
                                    list.chaos_insurgents++;
                                    break;
                                case Team.FoundationForces:
                                    list.mtf_and_guards++;
                                    break;
                                case Team.SCPs:
                                    {
                                        if (pl.RoleInformation.Role is RoleTypeId.Scp0492) list.zombies++;
                                        else list.scps_except_zombies++;
                                        break;
                                    }
                            }
                    }
                    catch { }
                }

                list.warhead_kills = AlphaWarheadController.Detonated ? AlphaWarheadController.Singleton.WarheadKills : -1;

                int scp = list.scps_except_zombies + list.zombies;
                int dboys = RoundSummary.EscapedClassD + list.class_ds;
                int scientists = RoundSummary.EscapedScientists + list.scientists;

                bool MTFAlive = list.mtf_and_guards > 0;
                bool CiAlive = list.chaos_insurgents > 0;
                bool ScpAlive = scp > 0;
                bool DClassAlive = list.class_ds > 0;
                bool ScientistsAlive = list.scientists > 0;
                bool HandAlive = sh > 0;

                int chaos_cf = 0;
                int mtf_cf = 0;
                int scp_cf = 0;

#if MRP
                if ((HandAlive || ScpAlive) && !MTFAlive && !DClassAlive && !ScientistsAlive && !CiAlive)
#elif NR
                if ((HandAlive || ScpAlive) && !MTFAlive && !DClassAlive && !ScientistsAlive)
#endif
                {
                    ev.End = true;
                    scp_cf++;
                }
                else if (!HandAlive && !ScpAlive && (MTFAlive || ScientistsAlive) && !DClassAlive && !CiAlive)
                {
                    ev.End = true;
                    mtf_cf++;
                }
                else if (!HandAlive && !ScpAlive && !MTFAlive && !ScientistsAlive && (DClassAlive || CiAlive))
                {
                    ev.End = true;
                    chaos_cf++;
                }
                else if (!ScpAlive && !MTFAlive && !ScientistsAlive && !DClassAlive && !CiAlive)
                {
                    ev.End = true;
                }

                if (!ev.End)
                    return;

                ev.Info = list;

                if (CO2.Activated)
                {
                    ev.Winner = RoundSummary.LeadingTeam.FacilityForces;
                    return;
                }
                if (ControlRoom.Activated)
                {
                    ev.Winner = RoundSummary.LeadingTeam.Anomalies;
                    return;
                }
                if (Concepts.NuclearAttack.Builds.Activated)
                {
                    ev.Winner = RoundSummary.LeadingTeam.Draw;
                    return;
                }
                if (OmegaWarhead.Detonated || OmegaWarhead.InProgress)
                {
                    ev.Winner = RoundSummary.LeadingTeam.ChaosInsurgency;
                    return;
                }

                var winner = RoundSummary.LeadingTeam.Draw;

                if (dboys > scientists) chaos_cf++;
                else if (dboys < scientists) mtf_cf++;
                else if (scp > dboys + scientists) scp_cf++;

                if (list.chaos_insurgents > list.mtf_and_guards) chaos_cf++;
                else if (list.chaos_insurgents < list.mtf_and_guards) mtf_cf++;
                else if (scp > list.chaos_insurgents + list.mtf_and_guards) scp_cf++;

                if (chaos_cf > mtf_cf)
                {
                    if (chaos_cf > scp_cf) winner = RoundSummary.LeadingTeam.ChaosInsurgency;
                    else if (mtf_cf < scp_cf) winner = RoundSummary.LeadingTeam.Anomalies;
                    else winner = RoundSummary.LeadingTeam.Draw;
                }
                else if (mtf_cf > chaos_cf)
                {
                    if (mtf_cf > scp_cf) winner = RoundSummary.LeadingTeam.FacilityForces;
                    else if (chaos_cf < scp_cf) winner = RoundSummary.LeadingTeam.Anomalies;
                    else winner = RoundSummary.LeadingTeam.Draw;
                }
                else winner = RoundSummary.LeadingTeam.Draw;

                ev.Winner = winner;
            }
            catch { }
        }
    }
}