using Qurre.API.Addons.Models;
using UnityEngine;

namespace Loli.Builds.Models
{
    internal class Radar
    {
        internal readonly Model Model;
        internal readonly Model Crutilka;
        internal readonly Model Tarelka;
        internal static readonly Color32 Color1 = new(46, 48, 48, 255); // server black
        internal static readonly Color32 Color2 = new(148, 150, 149, 255); // poloska servera
        internal static readonly Color32 Color3 = new(33, 33, 33, 255); // server bottom radiator
        internal static readonly Color32 Color4 = new(60, 62, 61, 255); // sektor color
        internal static readonly Color32 Color5 = new(150, 150, 150, 255); // server
        internal static readonly Color Red = new(4, 0, 0); // server
        internal Radar(Vector3 pos, Vector3 rot, Model root = null, Vector3 scale = default)
        {
            Model = new Model($"Radar_" + pos, pos, rot, (scale == default ? new Vector3(0.725f, 0.725f, 0.725f) : scale), root);
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, Color1, new Vector3(0, 2.54f), Vector3.zero, new Vector3(10, 0.5f, 10)));
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, Color2, new Vector3(0, 1.31f), Vector3.zero, new Vector3(8, 2, 8)));

            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, Color3, new Vector3(-2.2f, -0.81f), new Vector3(0, 0, -243), new Vector3(4, 2, 4)));
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, Color3, new Vector3(2.2f, -0.81f), new Vector3(0, 0, 243), new Vector3(4, 2, 4)));

            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, Color3, new Vector3(0.36f, -0.81f, 2.2f), new Vector3(0, 90, -243), new Vector3(4, 2, 4)));
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, Color3, new Vector3(0.36f, -0.81f, -2.2f), new Vector3(0, 90, 243), new Vector3(4, 2, 4)));

            foreach (var prim in Model.Primitives)
            {
                prim.Primitive.IsStatic = true;
            }


            Crutilka = new Model($"Crutilka", new(0, 4.4f), Vector3.zero, Model);
            Crutilka.AddPart(new ModelPrimitive(Crutilka, PrimitiveType.Cube, Color4, new Vector3(-1.34f, -0.36f), new Vector3(0, 0, -54), new Vector3(3, 0.5f, 1)));
            Crutilka.AddPart(new ModelPrimitive(Crutilka, PrimitiveType.Cube, Color4, new Vector3(1.34f, -0.36f), new Vector3(0, 0, 54), new Vector3(3, 0.5f, 1)));

            Crutilka.AddPart(new ModelPrimitive(Crutilka, PrimitiveType.Cube, Color4, new Vector3(0, -0.36f, -1.34f), new Vector3(0, 90, 54), new Vector3(3, 0.5f, 1)));
            Crutilka.AddPart(new ModelPrimitive(Crutilka, PrimitiveType.Cube, Color4, new Vector3(0, -0.36f, 1.34f), new Vector3(0, 90, -54), new Vector3(3, 0.5f, 1)));

            Crutilka.AddPart(new ModelPrimitive(Crutilka, PrimitiveType.Cube, Color4, new Vector3(-1.221f, 1.628f), new Vector3(0, 0, 41.3f), new Vector3(3, 0.5f, 1)));
            Crutilka.AddPart(new ModelPrimitive(Crutilka, PrimitiveType.Cube, Color4, new Vector3(0, 1.684f, 1.276f), new Vector3(0, 90, 41.3f), new Vector3(3, 0.5f, 1)));

            Crutilka.AddPart(new ModelPrimitive(Crutilka, PrimitiveType.Cube, Color5, new Vector3(0.724f, 1.833f, -0.732f), new Vector3(0, 46.27f, 45), new Vector3(0.2f, 3, 3)));


            var _tarelka = new Model($"DoTarelka", new(0.82f, 1.973f, -0.833f), new(0, 46.27f, 45), Crutilka);
            Tarelka = new Model($"Tarelka", Vector3.zero, Vector3.zero, _tarelka);
            Tarelka.AddPart(new ModelPrimitive(Tarelka, PrimitiveType.Cube, Color5, Vector3.zero, Vector3.zero, new Vector3(0.2f, 3, 3)));
            CreatePanel(new(0.249f, 0), Vector3.zero);
            CreatePanel(new(0.936f, 3.9f), new(0, 0, -20));
            CreatePanel(new(0.936f, -3.9f), new(0, 0, 20));
            Tarelka.AddPart(new ModelPrimitive(Tarelka, PrimitiveType.Cube, Color.white, new Vector3(2.54f, -3.22f), new Vector3(0, 0, 50), new Vector3(4, 0.4f, 0.4f)));
            Tarelka.AddPart(new ModelPrimitive(Tarelka, PrimitiveType.Cube, Color.white, new Vector3(3.32f, 1.82f, 2.73f), new Vector3(0, 73.2f, -49f), new Vector3(7.9f, 0.4f, 0.4f)));
            Tarelka.AddPart(new ModelPrimitive(Tarelka, PrimitiveType.Cube, Color.white, new Vector3(3.32f, 1.82f, -2.73f), new Vector3(0, -73.2f, -49f), new Vector3(7.9f, 0.4f, 0.4f)));
            Tarelka.AddPart(new ModelPrimitive(Tarelka, PrimitiveType.Sphere, Red, new Vector3(4.043f, -1.432f), Vector3.zero, Vector3.one));
            void CreatePanel(Vector3 _pos, Vector3 _rot)
            {
                var _panel = new Model($"Panel", _pos, _rot, Tarelka);
                Tarelka.AddPart(new ModelPrimitive(_panel, PrimitiveType.Cube, Color.white, Vector3.zero, Vector3.zero, new Vector3(0.3f, 4, 4)));
                Tarelka.AddPart(new ModelPrimitive(_panel, PrimitiveType.Cube, Color.white, new Vector3(0.674f, 0, 3.83f), new Vector3(0, 20), new Vector3(0.3f, 4, 4)));
                Tarelka.AddPart(new ModelPrimitive(_panel, PrimitiveType.Cube, Color.white, new Vector3(0.674f, 0, -3.83f), new Vector3(0, -20), new Vector3(0.3f, 4, 4)));
            }
            Crutilka.GameObject.AddComponent<KrutilkaScript>();
            Tarelka.GameObject.AddComponent<TarelkaScript>();

            foreach (var prim in Crutilka.Primitives)
            {
                prim.Primitive.MovementSmoothing = 64;
            }
            foreach (var prim in Tarelka.Primitives)
            {
                prim.Primitive.MovementSmoothing = 64;
            }
        }

        internal class KrutilkaScript : MonoBehaviour
        {
            readonly float _interval = 0.3f;
            float _nextCycle = 0f;

            void Start()
            {
                _nextCycle = Time.time;
                transform.Rotate(Vector3.down * Random.Range(0, 360));
            }

            void Update()
            {
                if (Time.time < _nextCycle)
                    return;

                _nextCycle += _interval;

                transform.Rotate(Vector3.down);
            }
        }

        internal class TarelkaScript : MonoBehaviour
        {
            readonly float _interval = 0.5f;
            float _nextCycle = 0f;

            void Start()
            {
                _nextCycle = Time.time;
                transform.Rotate(Vector3.left * Random.Range(0, 360));
            }

            void Update()
            {
                if (Time.time < _nextCycle)
                    return;

                _nextCycle += _interval;

                transform.Rotate(Vector3.left);
            }
        }
    }
}