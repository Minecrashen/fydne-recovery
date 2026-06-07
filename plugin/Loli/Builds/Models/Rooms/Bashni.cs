using System.Collections.Generic;
using System.IO;
using MEC;
using Qurre.API;
using Qurre.API.Controllers;
using Qurre.API.Objects;
using SchematicUnity.API;
using SchematicUnity.API.Objects;
using UnityEngine;

namespace Loli.Builds.Models.Rooms;

internal static class Bashni
{
    internal static void Load()
    {
        // Right A
        new Lift(new Lift.LiftCreator(null, new Vector3(-18.51f, 313.819f, -51.713f), new Vector3(0, 90), true),
            new Lift.LiftCreator(null, new Vector3(-12.02f, 301.813f, -51.713f), new Vector3(0, 90), true), new Color32(150, 150, 150, 255));

        // Right B
        new Lift(new Lift.LiftCreator(null, new Vector3(-9.638248f, 313.819f, -34.34925f), new Vector3(0, 180), true),
            new Lift.LiftCreator(null, new Vector3(-6.520752f, 301.813f, -31.23175f), new Vector3(0, 90), true), new Color32(150, 150, 150, 255));

        // Gate3 <-> Shooting
        new Lift(new Lift.LiftCreator(null, new Vector3(-128.359f, 292.91f, -27.84f), new Vector3(0, 180), true),
            new Lift.LiftCreator(null, new Vector3(-16.4025f, 300.109f, -42.52072f), new Vector3(0, 0), true),
            new Color32(143, 148, 154, 255));

        Scheme Scheme = SchematicManager.LoadSchematic(Path.Combine(Paths.Plugins, "Schemes", "Bashni.json"), new(0, -700));
        foreach (SObject _obj in Scheme.Objects)
            findObjects(_obj);

        new Locker(new(-16.44844f, 298.4458f, -36.389f), LockerPrefabs.RifleRack, Quaternion.Euler(new(0, 180)));
        new WorkStation(new(-18.383f, 298.3318f, -37.564f), new Vector3(0, 90), Vector3.one);
        new Radar(new(-14.63f, 317.75f, -52f), Vector3.zero);

        static void findObjects(SObject obj)
        {
            if (obj is null)
                return;

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
                case "Potolok":
                    {
                        if (obj.Primitive != null)
                        {
                            PrimitiveParams prm = (PrimitiveParams)obj.Primitive;
                            Timing.RunCoroutine(NeonLight(prm));
                        }

                        break;
                    }
                case "WithoutCollider":
                    {
                        if (obj.Primitive != null)
                        {
                            PrimitiveParams prm = (PrimitiveParams)obj.Primitive;
                            prm.Base.IsStatic = true;
                            prm.Base.Collider = false;
                        }

                        break;
                    }
                default:
                    {
                        if (obj.Primitive != null)
                        {
                            PrimitiveParams prm = (PrimitiveParams)obj.Primitive;
                            prm.Base.IsStatic = true;
                        }

                        break;
                    }
            }

            if (obj.Childrens is null)
                return;

            foreach (SObject _obj in obj.Childrens)
                findObjects(_obj);
        }
    }

    private static IEnumerator<float> NeonLight(PrimitiveParams prim)
    {
        bool plus = false;
        Color color = new(0, 0, 6);
        for (; ; )
        {
            if (plus) color.b += 0.2f;
            else color.b -= 0.2f;
            if (color.b < 1) plus = true;
            else if (color.b > 6) plus = false;
            prim.Color = color;
            yield return Timing.WaitForSeconds(0.1f);
        }
    }
}