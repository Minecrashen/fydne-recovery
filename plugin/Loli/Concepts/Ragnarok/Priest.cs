#if NR
using Loli.Addons;
using Loli.Addons.Hints;
using Loli.DataBase.Modules;
using Loli.DataBase.Modules.Controllers;
using Loli.HintsCore;
using Loli.HintsCore.Utils;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using Qurre.API;
using Qurre.API.Addons.Models;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using SchematicUnity.API.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LabApi.Features.Wrappers;
using UnityEngine;
using Player = Qurre.API.Controllers.Player;
using Round = Qurre.API.World.Round;

namespace Loli.Concepts.Ragnarok
{
    static class Priest
    {
        internal const string Tag = "SpawnedPriestPlayer";
        internal const string Believer = "BelieverPlayer";
        internal const string MessageTag = "Loli.Concepts.Ragnarok.Priest.Message";

        static string DirectoryPath { get; } = Path.Combine(Path.Combine(Paths.Plugins, "Audio"), "Ragnarok");
        static internal string AudioPathStart { get; } = Path.Combine(DirectoryPath, "Start.raw");
        static internal string AudioPathCycle { get; } = Path.Combine(DirectoryPath, "Cycle.raw");


        static DateTime lastPray = DateTime.Now;
        static bool alreadyPray = false;

        static string CachedCounts = string.Empty;
        static readonly MessageBlock MessageBlock;


        [EventMethod(RoundEvents.Waiting)]
        static void NullCall()
        {
            alreadyPray = false;
        }

        static void MessageEvent(MessageEvent ev)
        {
            Vector3 roomPos;
            try { roomPos = ev.Player.GamePlay.Room.Position; }
            catch { roomPos = ev.Player.MovementState.Position; }

            int count = Player.List.Count(x => x.Tag.Contains(Believer) && Vector3.Distance(x.MovementState.Position, roomPos) < 11f);
            ev.MessageBlock.Content = CachedCounts + " | 📍 " + count;
        }

        static internal void Update1s()
        {
            int prCount = 0;
            int blCount = 0;

            foreach (Player pl in Player.List)
            {
                if (pl.Tag.Contains(Tag))
                    prCount++;
                else if (pl.Tag.Contains(Believer))
                    blCount++;
            }

            CachedCounts = $"⛪ {prCount} | 👤 {blCount}";
        }

        static Priest()
        {
            MessageBlock = new(string.Empty, new Color32(111, 111, 111, 255), "60%", MessageEvent);

            CommandsSystem.RegisterConsole("priest", SpawnPriestConsole);
            CommandsSystem.RegisterConsole("pri", SpawnPriestConsole);
            CommandsSystem.RegisterConsole("св", SpawnPriestConsole);

            CommandsSystem.RegisterConsole("believe", BelieveCommand);
            CommandsSystem.RegisterConsole("bel", BelieveCommand);
            CommandsSystem.RegisterConsole("уверовать", BelieveCommand);

            CommandsSystem.RegisterConsole("pray", PrayCommand);
            CommandsSystem.RegisterConsole("пр", PrayCommand);
            CommandsSystem.RegisterConsole("призыв", PrayCommand);

            static void SpawnPriestConsole(GameConsoleCommandEvent ev)
            {
                ev.Allowed = false;

                if (!Data.Roles.TryGetValue(ev.Player.UserInformation.UserId, out var data))
                {
                    ev.Reply = "Не удалось получить информацию из базы данных";
                    return;
                }

                if (!data.Priest)
                {
                    ev.Reply = "У вас не приобретен священник";
                    return;
                }

                if (ev.Player.Tag.Contains(Tag))
                {
                    ev.Reply = "Вы уже являетесь священником";
                    return;
                }

                if (ev.Player.RoleInformation.Team == Team.SCPs)
                {
                    ev.Reply = "За SCP нельзя стать священником";
                    return;
                }

                if (ev.Player.RoleInformation.Role is RoleTypeId.Spectator or RoleTypeId.Overwatch)
                {
                    ev.Reply = "Вы мертвы...";
                    return;
                }

                ev.Reply = "Перевоплащаюсь...";
                Spawn(ev.Player);
            }

            static void BelieveCommand(GameConsoleCommandEvent ev)
            {
                ev.Allowed = false;

                if (ev.Player.Tag.Contains(Tag))
                {
                    ev.Reply = "Священник не может уверовать";
                    return;
                }

                if (ev.Player.Tag.Contains(Believer))
                {
                    ev.Reply = "Вы уже уверовали";
                    return;
                }

                if (ev.Player.RoleInformation.Team == Team.SCPs)
                {
                    ev.Reply = "За SCP нельзя уверовать";
                    return;
                }

                ev.Reply = "Вы успешно уверовали";
                ev.Player.Tag += Believer;
            }

            static void PrayCommand(GameConsoleCommandEvent ev)
            {
                ev.Allowed = false;

                if (!ev.Player.Tag.Contains(Tag))
                {
                    ev.Reply = "Вы не Священник";
                    return;
                }

                if (Round.Waiting)
                {
                    ev.Reply = "Нельзя начать призыв в ожидании игроков";
                    return;
                }

                if (alreadyPray)
                {
                    ev.Reply = "Призыв уже начат или завершен";
                    return;
                }

                if (Round.ElapsedTime.TotalMinutes < 3)
                {
                    ev.Reply = "Начать призыв можно только после 3-х минут раунда";
                    return;
                }

                var room = ev.Player.GamePlay.Room;
                if (room.Type is not RoomType.LczCrossing and not RoomType.HczCrossing
                    and not RoomType.HczChkpA and not RoomType.HczChkpB
                    and not RoomType.EzCrossing)
                {
                    ev.Reply = "Данная комната не подходит для призыва";
                    return;
                }

                if ((DateTime.Now - lastPray).TotalSeconds < 1)
                {
                    ev.Reply = $"Рейт лимит использования команды, попробуйте снова через {1000 - (DateTime.Now - lastPray).TotalMilliseconds}ms";
                    return;
                }
                lastPray = DateTime.Now;

                Vector3 roomPos = Vector3.zero;
                try { roomPos = room.Position; }
                catch { roomPos = ev.Player.MovementState.Position; }

                var believers = Player.List.Where(x =>
                    (x.Tag.Contains(Believer) || x.Tag.Contains(Tag)) &&
                    Vector3.Distance(x.MovementState.Position, roomPos) < 11f
                    );

                if (believers.Count() < 3)
                {
                    ev.Reply = "Число Верующих и Священников около вас менее 3-x, включая вас";
                    return;
                }

                Timing.RunCoroutine(Coroutine());

                IEnumerator<float> Coroutine()
                {
                    alreadyPray = true;

                    // youtu.be/P9Hres93mBs
                    AudioExtensions.PlayFromPlayer(AudioPathStart, ev.Player, "Призыв");
                    if (roomPos != Vector3.zero)
                        AudioExtensions.PlayFromPosition(AudioPathStart, roomPos + Vector3.up, "Призыв");

                    Model model_room = new("Room", room.Position, room.Rotation.eulerAngles);
                    CreateCandle(new(2.45f, 0.0198f, 2.45f), model_room);
                    CreateCandle(new(-2.45f, 0.0198f, 2.45f), model_room);
                    CreateCandle(new(-2.45f, 0.0198f, -2.45f), model_room);
                    CreateCandle(new(2.45f, 0.0198f, -2.45f), model_room);

                    foreach (var believer in believers)
                    {
                        believer.Effects.Enable(EffectType.SeveredHands, 100);
                        believer.Effects.Enable(EffectType.Burned, 100);
                        believer.GamePlay.GodMode = true;
                    }

                    room.LightsOff(100f);
                    foreach (var door in room.Doors)
                    {
                        try
                        {
                            door.Lock = true;
                            door.Open = false;
                        }
                        catch { }
                    }

                    yield return Timing.WaitForSeconds(90f);

                    Model root = new("GodBox_root", room.Position, room.Rotation.eulerAngles);
                    Model box = new("GodBox", new(0, -3.2f), Vector3.zero, root);
                    Color32 stenki = new(47, 50, 52, 255);

                    box.AddPart(new ModelPrimitive(box, PrimitiveType.Cube, stenki, new(0, 1.538f, -2.283f), Vector3.zero, new(4.7f, 3, 0.1f)));
                    box.AddPart(new ModelPrimitive(box, PrimitiveType.Cube, stenki, new(0, 1.538f, 2.283f), Vector3.zero, new(4.7f, 3, 0.1f)));
                    box.AddPart(new ModelPrimitive(box, PrimitiveType.Cube, stenki, new(2.283f, 1.538f), Vector3.zero, new(0.1f, 3, 4.7f)));
                    box.AddPart(new ModelPrimitive(box, PrimitiveType.Cube, stenki, new(-2.283f, 1.538f), Vector3.zero, new(0.1f, 3, 4.7f)));

                    ModelPrimitive _boxTop = new(box, PrimitiveType.Cube, stenki, new(0, 3), Vector3.zero, new(4.7f, 0.1f, 4.7f));

                    for (float i = -3.2f; i < 0; i += 0.1f)
                    {
                        box.GameObject.transform.localPosition = new(0, i);
                        yield return Timing.WaitForSeconds(0.05f);
                    }
                    box.GameObject.transform.localPosition = Vector3.zero;

                    Timing.CallDelayed(1.5f, () =>
                    {
                        foreach (var prim in box.Primitives)
                        {
                            Force(prim);
                        }

                        Force(_boxTop);
                        Timing.CallDelayed(1f, () => _boxTop.Primitive.Destroy());

                        void Force(ModelPrimitive prim)
                        {
                            try
                            {
                                var r = prim.GameObject.AddComponent<Rigidbody>();
                                prim.Primitive.Collider = false;
                                r.AddExplosionForce(200, box.GameObject.transform.position, 10);
                            }
                            catch { }
                        }
                    });

                    Timing.CallDelayed(5f, () => box.Destroy());

                    var _scheme = SchematicUnity.API.SchematicManager.LoadSchematic(
                        Path.Combine(Paths.Plugins, "Schemes", "GodLocation.json"),
                        room.Position,
                        room.Rotation);

                    foreach (var _obj in _scheme.Objects)
                    {
                        try { findObjects(_obj); } catch { }
                    }
                    void findObjects(SObject obj)
                    {
                        switch (obj.Name)
                        {
                            case "GunPosition":
                                {
                                    try { Pickup.Create(Extensions.GetRandomGun(), obj.Transform.position, obj.Transform.rotation); } catch { }
                                    break;
                                }
                            case "Bible":
                                {
                                    foreach (var _obj in obj.Childrens)
                                    {
                                        try { findBibleObjects(_obj); } catch { }
                                    }
                                    void findBibleObjects(SObject obj)
                                    {
                                        try
                                        {
                                            if (obj.Primitive is PrimitiveParams prim && prim.Type == PrimitiveType.Quad)
                                            {
                                                prim.Base.Collider = false;
                                            }
                                        }
                                        catch { }
                                        foreach (var _obj in obj.Childrens)
                                        {
                                            try { findBibleObjects(_obj); } catch { }
                                        }
                                    }
                                    break;
                                }
                        }
                        foreach (var _obj in obj.Childrens)
                        {
                            try { findObjects(_obj); } catch { }
                        }
                    }

                    GameObject dead = new("Dead Coffin");
                    dead.transform.parent = root.GameObject.transform;
                    dead.transform.localPosition = new(0, 0.7f, 0.848f);
                    dead.transform.localRotation = Quaternion.Euler(new Vector3(-90, 0));
                    new Corpse(RoleTypeId.Scp3114, dead.transform.position, dead.transform.rotation, new CustomReasonDamageHandler("Распят на кресте ☦"), "[Неизвестно]");


                    // youtu.be/SEO6Bx0pbOs
                    // youtu.be/qI2yPR9pLEc
                    // 
                    if (roomPos != Vector3.zero)
                        AudioExtensions.PlayFromPosition(AudioPathCycle, roomPos + Vector3.up, "Хор");

                    yield return Timing.WaitForSeconds(4f);

                    foreach (var believer in believers)
                    {
                        believer.Effects.Disable(EffectType.SeveredHands);
                        believer.Effects.Disable(EffectType.Burned);
                        believer.GamePlay.GodMode = false;
                    }

                    yield return Timing.WaitForSeconds(5f);

                    room.Lights.Color = new Color32(245, 187, 73, 100);
                    room.Lights.LockChange = true;

                    foreach (var door in room.Doors)
                    {
                        try
                        {
                            door.Lock = false;
                        }
                        catch { }
                    }

                    yield break;
                }
            }

            /*
            RoomType.LczCrossing;
            RoomType.HczCrossing;
            RoomType.HczThreeWay;
            RoomType.HczChkpA;
            RoomType.HczChkpB;
            RoomType.EzCrossing;
            */
        }

        static void CreateCandle(Vector3 position, Model root = null)
        {
            Model Candle = new("Candle", position, Vector3.zero, root);

            Candle.AddPart(new ModelPrimitive(Candle,
                PrimitiveType.Cylinder,
                new Color32(214, 205, 180, 255),
                Vector3.zero, Vector3.zero,
                new(0.07f, 0.03f, 0.07f)
                ));

            Candle.AddPart(new ModelPrimitive(Candle,
                PrimitiveType.Capsule,
                new Color32(13, 12, 11, 255),
                new(0, 0.03246093f),
                new(-6.53f, -6.6f, -5f),
                new(0.005f, 0.01f, 0.005f)
                ));

            Candle.AddPart(new ModelLight(Candle, new Color32(245, 187, 73, 255), new(0, 0.0893f), 1.5f, 1, shadowStrength: 0));
        }

        static void Spawn(Player pl)
        {
            pl.Tag += Tag;

            MainInfo.UpdateMessage(pl, MessageBlock, true, MessageTag);

            new Glow(pl, Color.yellow);
            new Nimb(pl);

            pl.Inventory.AddItem(ItemType.Coin);

            pl.Client.Broadcast(10, $"<size=80%><color=#f2d410>Вы - Священник</color>\n" +
                $"<color=#6f6f6f>Туториал можно посмотреть в Server-Info</color></size>", true);
            pl.UserInformation.CustomInfo = "Священник";
            pl.UserInformation.InfoToShow = PlayerInfoArea.Nickname | PlayerInfoArea.Badge | PlayerInfoArea.CustomInfo | PlayerInfoArea.PowerStatus;
        }




        [EventMethod(PlayerEvents.Dead)]
        static void DeadPriest(DeadEvent ev)
        {
            DeadPriest(ev.Target);
            DeadBeliever(ev.Target);
        }

        [EventMethod(PlayerEvents.ChangeRole)]
        static void DeadPriest(ChangeRoleEvent ev)
        {
            DeadPriest(ev.Player);
            DeadBeliever(ev.Player);
        }

        static void DeadPriest(Player pl)
        {
            if (pl is null)
                return;

            if (!pl.Tag.Contains(Tag))
                return;

            pl.Tag = pl.Tag.Replace(Tag, "");

            MainInfo.UpdateMessage(pl, MessageBlock, false, MessageTag);

            if (Nimb.TryGet(pl, out var nimb))
                nimb.Destroy();

            if (Glow.TryGet(pl, out var glow))
                glow.Destroy();
        }

        static void DeadBeliever(Player pl)
        {
            if (pl is null)
                return;

            if (!pl.Tag.Contains(Believer))
                return;

            pl.Tag = pl.Tag.Replace(Believer, "");
        }
    }
}
#endif