using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using SchematicUnity.API.Objects;
using System.IO;
using Qurre.API.Controllers;
using UnityEngine;

namespace Loli.Builds.Models.Rooms
{
    static class Gate3
    {
        [EventMethod(RoundEvents.Waiting)]
        static internal void Load()
        {
#if MRP
            var Scheme = SchematicUnity.API.SchematicManager.LoadSchematic(Path.Combine(Paths.Plugins, "Schemes", "Gate3Mrp.json"), new(0, -700));
#elif NR
            var Scheme = SchematicUnity.API.SchematicManager.LoadSchematic(Path.Combine(Paths.Plugins, "Schemes", "Gate3.json"), new(0, -700));
#endif

            foreach (var _obj in Scheme.Objects)
                findObjects(_obj);
            static void findObjects(SObject obj)
            {
                if (obj is null)
                    return;

                if (obj.Primitive != null)
                {
                    PrimitiveParams prm = (PrimitiveParams)obj.Primitive;
                    prm.Base.IsStatic = true;
                }

                switch (obj.Name)
                {
                    case "Steklo":
                        {
                            if (obj.Primitive != null)
                            {
                                PrimitiveParams prm = (PrimitiveParams)obj.Primitive;
                                prm.Color = new Color32(0, 133, 155, 150);
                            }
                            break;
                        }
                    case "DoorGate3Exit":
                        {
                            if (obj.Primitive != null)
                            {
                                PrimitiveParams prm = (PrimitiveParams)obj.Primitive;
                                var scr = prm.Base.Base.gameObject.AddComponent<OnDoorScript>();
                                scr._type = 1;
                            }
                            break;
                        }
                    case "DoorGate3Enter":
                        {
                            if (obj.Primitive != null)
                            {
                                PrimitiveParams prm = (PrimitiveParams)obj.Primitive;
                                var scr = prm.Base.Base.gameObject.AddComponent<OnDoorScript>();
                                scr._type = 2;
                            }
                            break;
                        }
                }

                if (obj.Childrens is null)
                    return;

                foreach (var _obj in obj.Childrens)
                    findObjects(_obj);
            }
            try
            {
                //new Lamp(new(-135.0814f, 991.5962f, -6.769f), Vector3.zero);//1
                new Lamp(new(-135.08f, 291.5962f, -47.5f), Vector3.zero);//2
                new Lamp(new(-104.3564f, 291.8444f, -52.4819f), Vector3.zero);//3
                new Lamp(new(-104.3564f, 291.8444f, -33.6771f), Vector3.zero);//4
                new Lamp(new(-56.37352f, 291.4794f, -52.71039f), Vector3.zero);//5
                new Lamp(new(-44.4665f, 287.7709f, -38.0133f), Vector3.zero);//6
                new Lamp(new(-71.6298f, 287.7709f, -38.1228f), Vector3.zero);//7
                new Lamp(new(-8.630798f, 295.1732f, -11.799f), Vector3.zero);//8
                new Lamp(new(10.247f, 295.1732f, -3.732502f), Vector3.zero);//9
            }
            catch { }
        }

        [EventMethod(PlayerEvents.Damage)]
        internal static void AntiMachineDead(DamageEvent ev)
        {
            if (ev.DamageType == DamageTypes.Crushed &&
                ev.Target.MovementState.Position.y < 300 &&
                ev.Target.MovementState.Position.y > 270)
                ev.Allowed = false;
        }

        static Vector3 DoorExit_Tp = new(-39.6f, 293, -36.04f);
        static Vector3 DoorEntrance_Tp = new(-58.345f, 288.723f, -36.184f);

        public class OnDoorScript : MonoBehaviour
        {
            internal int _type = 0;
            private void Start()
            {
                var collider = gameObject.AddComponent<BoxCollider>();
                collider.isTrigger = true;
                collider.size = Vector3.one * 2f;
                collider.center = Vector3.up;
            }

            private void OnTriggerEnter(Collider other)
            {
                if (_type != 1 && _type != 2)
                    return;

                if (!other.gameObject.name.Contains("Player"))
                    return;

                Player pl = other.gameObject.GetPlayer();
                if (_type == 1)
                    pl.MovementState.Position = DoorExit_Tp;
                else if (_type == 2)
                    pl.MovementState.Position = DoorEntrance_Tp;
            }
        }
    }
}