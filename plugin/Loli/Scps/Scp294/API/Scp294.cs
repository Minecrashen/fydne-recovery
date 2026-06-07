using Loli.Addons;
using Qurre.API;
using Qurre.API.Addons.Models;
using Qurre.Events.Structs;
using UnityEngine;

namespace Loli.Scps.Scp294.API
{
    public sealed class Scp294
    {
        static void Event(InteractWorkStationEvent ev)
        {
            ev.Allowed = false;
            Events.Interact(ev.Player);
        }

        private static readonly Color32 DarkColor = new(39, 39, 39, 255);
        private static readonly Color32 DarkGrayColor = new(77, 77, 77, 255);
        private static readonly Color32 GrayColor = new(200, 200, 200, 255);
        private static readonly Color32 ScreenColor = new(92, 166, 156, 255);
        private static readonly Color32 GreenMiniScreenColor = new(146, 192, 144, 255);
        private static readonly Color32 GlassColor = new(133, 120, 229, 75);

        public Model Model { get; private set; }

        public GameObject GameObject => Model?.GameObject;
        public Transform Transform => GameObject?.transform;

        public void Spawn(Vector3 position, Vector3 rotation)
        {
            try
            {
                Model = new Model("SCP-294", position, rotation, new Vector3(0.725f, 0.725f, 0.725f));
                // Ножки
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cylinder, DarkColor, new Vector3(0.5567f, -1.4776f, 0.6548f), Vector3.zero, new Vector3(0.075f, 0.1f, 0.075f)));
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cylinder, DarkColor, new Vector3(-0.587f, -1.4776f, 0.6548f), Vector3.zero, new Vector3(0.075f, 0.1f, 0.075f)));
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cylinder, DarkColor, new Vector3(0.5567f, -1.4776f, -0.661f), Vector3.zero, new Vector3(0.075f, 0.1f, 0.075f)));
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cylinder, DarkColor, new Vector3(-0.587f, -1.4776f, -0.6609959f), Vector3.zero, new Vector3(0.075f, 0.1f, 0.075f)));
                // Корпус
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, DarkGrayColor, Vector3.up * 1.3296f, Vector3.zero, new Vector3(1.450443f, 0.1151492f, 1.632817f)));
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, DarkGrayColor, Vector3.down * 1.1007f, Vector3.zero, new Vector3(1.450443f, 0.5732275f, 1.632817f)));
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, DarkGrayColor, new Vector3(0, 0.2263f, 0.5977f), Vector3.zero, new Vector3(1.450443f, 2.104969f, 0.4376237f)));
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, DarkGrayColor, new Vector3(0, 0.2263f, -0.7239f), Vector3.zero, new Vector3(1.450443f, 2.104969f, 0.1850592f)));
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, DarkGrayColor, new Vector3(0.0525f, 0.28f, -0.1259f), Vector3.zero, new Vector3(1.345512f, 0.114085f, 1.011355f)));
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, DarkGrayColor, new Vector3(-0.149f, 0.2262f, -0.1245f), Vector3.zero, new Vector3(1.1523f, 2.099451f, 1.034673f)));
                // Экран
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Capsule, ScreenColor, new Vector3(0.375f, 0.8037f, -0.1364f), Vector3.right * 90, new Vector3(0.4463202f, 0.6184692f, 0.9733779f)));
                // Клавиатура
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, DarkColor, new Vector3(0.5877f, 0.41f, -0.1259f), Vector3.back * 60, new Vector3(0.2749956f, 0.114085f, 1.011355f)));
                // Нижний блок
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, GrayColor, new Vector3(0.62924f, -0.2968f, 0.0753f), Vector3.zero, new Vector3(0.06477465f, 1.063232f, 0.6265934f)));
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, GrayColor, new Vector3(0.62924f, -0.2968f, -0.5958f), Vector3.zero, new Vector3(0.06477465f, 1.063232f, 0.08947857f)));
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, GrayColor, new Vector3(0.62924f, 0.0282f, -0.3938f), Vector3.zero, new Vector3(0.06477465f, 0.4132853f, 0.3270303f)));
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, GrayColor, new Vector3(0.62924f, -0.6846f, -0.3943f), Vector3.zero, new Vector3(0.06477477f, 0.2877299f, 0.3175081f)));

                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, DarkGrayColor, new Vector3(0.5087f, -0.2968f, 0.0753f), Vector3.zero, new Vector3(0.1801007f, 1.063232f, 0.6265934f)));
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, DarkGrayColor, new Vector3(0.5087f, -0.2968f, -0.5958f), Vector3.zero, new Vector3(0.1801008f, 1.063232f, 0.08947857f)));
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, DarkGrayColor, new Vector3(0.5087f, 0.0282f, -0.3938f), Vector3.zero, new Vector3(0.1801008f, 0.4132853f, 0.3270303f)));
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, DarkGrayColor, new Vector3(0.5087f, -0.6846f, -0.3943f), Vector3.zero, new Vector3(0.1801011f, 0.2877299f, 0.3175081f)));
                // Зелёный мини дисплей
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, GreenMiniScreenColor, new Vector3(0.71989f, 0.7326f, 0.5921f), Vector3.zero, new Vector3(0.02554291f, 0.04240394f, 0.2504892f)));
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, GreenMiniScreenColor, new Vector3(0.71989f, 0.62915f, 0.5921f), Vector3.zero, new Vector3(0.02554291f, 0.05897837f, 0.2504892f)));
                // Стеклянная панель
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, GlassColor, new Vector3(0.62615f, -0.20414f, -0.3944f), Vector3.zero, new Vector3(0.05575562f, 0.06101107f, 0.3084731f)));
                // Свет
                Model.AddPart(new ModelLight(Model, new Color32(245, 201, 161, 255), new Vector3(0.5082f, -0.3725f, -0.439f), 0.77f, 1f));
                Model.AddPart(new ModelLight(Model, ScreenColor, new Vector3(1.079f, 0.953f, -0.252f), 2f, 1f));
                Model.AddPart(new ModelLight(Model, GreenMiniScreenColor, new Vector3(0.7454376f, 0.7326f, 0.592041f), 1f, 1f));
                // Монетоприёмник (ДЕКОРАЦИЯ)
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, Color.black, new Vector3(0.7111206f, 0.6138275f, 0.6800537f), Vector3.zero, new Vector3(0.03126527f, 0.07567288f, 0.03832996f)));
                // Ключ карта
                ModelWorkStation station = new(Model, new(0.7253f, 0.6034f, 0.5946f), new(0, 90, 0), new(0.11f, 0.058f, 0.005f));
                Model.AddPart(station);
                StationsManager.Register(station.WorkStation, Event);
            }
            catch (System.Exception e)
            {
                Log.Error("\nERROR\n" + e);
            }
        }

        public void Destroy()
        {
            Model?.Destroy();
        }
    }
}