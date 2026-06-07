using Qurre.API;
using Qurre.API.Controllers;
using Qurre.API.Objects;
using SchematicUnity.API.Objects;
using System.IO;
using UnityEngine;

namespace Loli.Builds.Models.Rooms
{
    static class RadarLoc
    {
        static internal void Load()
        {
            var Scheme = SchematicUnity.API.SchematicManager.LoadSchematic(Path.Combine(Paths.Plugins, "Schemes", "RadarLoc.json"), new(0, -700));

            new Lamp(new(34.26f, 301.955f, -47.62f), Vector3.zero);
            new Radar(new(24.64f, 307.01f, -41.46f), Vector3.zero);

            new Door(new(31.419f, 301.242f, -41.50193f), DoorPrefabs.DoorEZ, Quaternion.Euler(new(0, 90)));

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
                    case "RedColor":
                        {
                            if (obj.Primitive != null)
                            {
                                PrimitiveParams prm = (PrimitiveParams)obj.Primitive;
                                prm.Color = new Color(10, 0, 0);
                            }
                            break;
                        }
                }

                if (obj.Childrens is null)
                    return;

                foreach (var _obj in obj.Childrens)
                    findObjects(_obj);
            }
        }
    }
}