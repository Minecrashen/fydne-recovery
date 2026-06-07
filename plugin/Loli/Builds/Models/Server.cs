using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using MEC;
using Mirror;
using Qurre.API;
using Qurre.API.Addons.Models;
using System.Collections.Generic;
using UnityEngine;

namespace Loli.Builds.Models
{
    internal class Server
    {
        internal readonly Model Model;
        internal readonly Model Door;
        internal static readonly Dictionary<ItemPickupBase, Server> Doors = new();
        private bool Opened = false;
        private bool Opening = false;
        private readonly GameObject Item;
        internal void InteractDoor()
        {
            if (Opening) return;
            if (Opened) Timing.RunCoroutine(CloseDoor(), "DoorOpenCloseInServer");
            else Timing.RunCoroutine(OpenDoor(), "DoorOpenCloseInServer");
            IEnumerator<float> OpenDoor()
            {
                Opening = true;
                try { NetworkServer.UnSpawn(Item); } catch { }
                Door.GameObject.transform.localRotation = Quaternion.Euler(new Vector3(0, 360, 0));
                for (float i = 360; i > 270; i--)
                {
                    try { Door.GameObject.transform.localRotation = Quaternion.Euler(new Vector3(0, i, 0)); } catch { }
                    yield return Timing.WaitForSeconds(0.05f);
                }
                Opened = true;
                Opening = false;
                try { NetworkServer.Spawn(Item); } catch { }
            }
            IEnumerator<float> CloseDoor()
            {
                Opening = true;
                try { NetworkServer.UnSpawn(Item); } catch { }
                Door.GameObject.transform.localRotation = Quaternion.Euler(new Vector3(0, 270, 0));
                for (float i = 270; i < 360; i++)
                {
                    try { Door.GameObject.transform.localRotation = Quaternion.Euler(new Vector3(0, i, 0)); } catch { }
                    yield return Timing.WaitForSeconds(0.05f);
                }
                Opened = false;
                Opening = false;
                try { NetworkServer.Spawn(Item); } catch { }
            }
        }
        internal Server(Vector3 pos, Vector3 rot, Model root = null)
        {
            Vector3 scale = new(0.725f, 0.725f, 0.725f);
            try { if (root.GameObject.transform.lossyScale.x < 0.8) scale = Vector3.one; } catch { }
            Model = new Model($"Server_" + pos, pos, rot, scale, root);
            Color32 main = new(45, 45, 43, 255);
            Color32 serverIn = new(85, 85, 85, 255);
            Color32 serverCol = new(150, 150, 150, 255);
            Color32 steklo = new(0, 133, 155, 150);
            Color32 steklo_black = new(0, 0, 0, 150);
            var _cyan = new Color(0, 4, 4);
            var _red = new Color(4, 0, 0);
            var _yellow = new Color(4, 4, 0);
            var _lime = new Color(0, 4, 0);
            #region Main
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, main, new Vector3(-0.025f, -0.8f), Vector3.zero, new Vector3(2.05f, 0.1f, 1)), false);
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Quad, main, new Vector3(1, 0.698f), new Vector3(0, 270), new Vector3(1, 3, 0.01f)), false);
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Quad, main, new Vector3(-0.023f, 0.698f, -0.5f), Vector3.zero, new Vector3(2.047f, 3, 0.01f)), false);
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Quad, main, new Vector3(0, 0.698f, 0.5f), new Vector3(0, 180), new Vector3(2, 3, 0.01f)), false);
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Quad, main, new Vector3(-0.847f, 0.698f, -0.5f), new Vector3(0, 180), new Vector3(0.4f, 3, 0.01f)), false);
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Quad, main, new Vector3(-0.847f, 0.698f, 0.5f), Vector3.zero, new Vector3(0.3f, 3, 0.01f)), false);
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Quad, main, new Vector3(-0.7f, 0.698f), RotForQuad, new Vector3(1, 3, 0.01f)), false);
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Quad, main, new Vector3(0.15f, 2.197f), new Vector3(90, 90), new Vector3(1, 1.7f, 0.01f)), false);
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, main, new Vector3(-0.8745f, 2.1726f), Vector3.zero, new Vector3(0.35f, 0.05f, 1)), false);
            #endregion
            #region Door
            {
                Color32 _color = new(59, 60, 62, 255);
                var Door = new Model("Door", new Vector3(-1.0245f, 0.7f, -0.472f), Vector3.zero, Model);
                Door.AddPart(new ModelPrimitive(Door, PrimitiveType.Cylinder, _color, Vector3.zero, Vector3.zero, new Vector3(0.05f, 1.45f, 0.05f)));
                Door.AddPart(new ModelPrimitive(Door, PrimitiveType.Cube, _color, new Vector3(0, 0, 0.05f), Vector3.zero, new Vector3(0.06f, 2.9f, 0.05f)));
                Door.AddPart(new ModelPrimitive(Door, PrimitiveType.Cube, _color, new Vector3(0, 0, 0.9464f), Vector3.zero, new Vector3(0.06f, 2.9f, 0.05f)));
                Door.AddPart(new ModelPrimitive(Door, PrimitiveType.Cube, _color, new Vector3(0, -1.403f, 0.501f), Vector3.zero, new Vector3(0.06f, 0.1f, 0.85f)));
                Door.AddPart(new ModelPrimitive(Door, PrimitiveType.Cube, _color, new Vector3(0, 1.3983f, 0.501f), Vector3.zero, new Vector3(0.06f, 0.1f, 0.85f)));
                Door.AddPart(new ModelPrimitive(Door, PrimitiveType.Cylinder, _color, new Vector3(0.001f, 0, 0.861f), new Vector3(0, 0, 90), new Vector3(0.15f, 0.034f, 0.15f)));
                Door.AddPart(new ModelPrimitive(Door, PrimitiveType.Quad, steklo, new Vector3(-0.029f, 0, 0.499f), RotForQuad, new Vector3(0.855f, 2.71f, 0.01f)));
                Door.AddPart(new ModelPrimitive(Door, PrimitiveType.Quad, steklo, new Vector3(0.029f, 0, 0.499f), new Vector3(0, 270, 0), new Vector3(0.855f, 2.71f, 0.01f)));
                var _p = new Vector3(-0.005f, 0.0023f, 0.8598f);


                var item = Qurre.API.Server.InventoryHost.CreateItemInstance(new(ItemType.SCP500, ItemSerialGenerator.GenerateNext()), false);
                ushort ser = ItemSerialGenerator.GenerateNext();

                item.PickupDropModel.Info.Serial = ser;
                item.PickupDropModel.Info.ItemId = ItemType.SCP500;
                item.PickupDropModel.Info.WeightKg = item.Weight;
                item.PickupDropModel.NetworkInfo = item.PickupDropModel.Info;

                ItemPickupBase ipb = Object.Instantiate(item.PickupDropModel);

                ipb.Position = Door.GameObject.transform.position + _p;
                ipb.Rotation = Quaternion.Euler(Door.GameObject.transform.rotation.eulerAngles + new Vector3(0, 0, 90));

                var gameObject = ipb.gameObject;
                gameObject.GetComponent<Rigidbody>().isKinematic = true;
                gameObject.transform.parent = Door.GameObject.transform;
                gameObject.transform.localPosition = _p;
                gameObject.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
                gameObject.transform.localScale = new Vector3(1.15f, 0.33f, 1.15f);

                NetworkServer.Spawn(gameObject);

                this.Door = Door;
                if (!Doors.ContainsKey(ipb))
                    Doors.Add(ipb, this);
                Item = gameObject;
            }
            #endregion
            #region TopServers
            {
                int _id = 0;
                var TopServers = new Model("TopServers", new Vector3(-0.8246f, 1.685f, 0.02f), Vector3.zero, Model);
                Model.AddPart(new ModelPrimitive(TopServers, PrimitiveType.Cube, serverIn, new Vector3(0.008f, 0), Vector3.zero, new Vector3(0.25f, 0.82f, 0.95f)));
                CreateInTopServer(new Vector3(-0.13f, 0.345f, 0));
                CreateInTopServer(new Vector3(-0.13f, 0.23f, 0));
                CreateInTopServer(new Vector3(-0.13f, 0.115f, 0));
                CreateInTopServer(new Vector3(-0.13f, 0, 0));
                CreateInTopServer(new Vector3(-0.13f, -0.115f, 0));
                CreateInTopServer(new Vector3(-0.13f, -0.23f, 0));
                CreateInTopServer(new Vector3(-0.13f, -0.345f, 0));
                void CreateInTopServer(Vector3 pos)
                {
                    _id++;
                    var TopServer = new Model($"TopServer #{_id}", pos, Vector3.zero, TopServers);
                    Model.AddPart(new ModelPrimitive(TopServer, PrimitiveType.Cube, serverCol, new Vector3(0.008f, 0), Vector3.zero, new Vector3(0.01f, 0.1f, 0.9f)));
                    Vector3 _r = new(0, 0, 90);
                    float _x = -0.0052f;
                    Model.AddPart(new ModelPrimitive(TopServer, PrimitiveType.Cylinder, _yellow, new Vector3(_x, 0, -0.4132f), _r, new Vector3(0.025f, 0.001f, 0.025f)));
                    Model.AddPart(new ModelPrimitive(TopServer, PrimitiveType.Cylinder, _cyan, new Vector3(_x, 0, -0.3823f), _r, new Vector3(0.012f, 0.001f, 0.012f)));
                    Model.AddPart(new ModelPrimitive(TopServer, PrimitiveType.Cylinder, _cyan, new Vector3(_x, 0, -0.3615f), _r, new Vector3(0.012f, 0.001f, 0.012f)));
                    Model.AddPart(new ModelPrimitive(TopServer, PrimitiveType.Cylinder, _cyan, new Vector3(_x, 0, -0.3418f), _r, new Vector3(0.012f, 0.001f, 0.012f)));
                    Model.AddPart(new ModelPrimitive(TopServer, PrimitiveType.Cylinder, _cyan, new Vector3(_x, 0, -0.3219f), _r, new Vector3(0.012f, 0.001f, 0.012f)));
                    Model.AddPart(new ModelPrimitive(TopServer, PrimitiveType.Cylinder, _cyan, new Vector3(_x, 0, -0.301f), _r, new Vector3(0.012f, 0.001f, 0.012f)));
                    Model.AddPart(new ModelPrimitive(TopServer, PrimitiveType.Cylinder, _red, new Vector3(_x, 0, -0.2727f), _r, new Vector3(0.025f, 0.001f, 0.025f)));
                    Vector3 _s = new(0.02f, 0.02f, 0.01f);
                    SetLight(PrimitiveType.Quad, _cyan, new Vector3(_x, 0.03f, 0.421f), RotForQuad, _s);
                    SetLight(PrimitiveType.Quad, _cyan, new Vector3(_x, 0, 0.421f), RotForQuad, _s);
                    SetLight(PrimitiveType.Quad, _cyan, new Vector3(_x, -0.033f, 0.421f), RotForQuad, _s);

                    SetLight(PrimitiveType.Quad, _cyan, new Vector3(_x, 0.03f, 0.387f), RotForQuad, _s);
                    SetLight(PrimitiveType.Quad, _cyan, new Vector3(_x, 0, 0.387f), RotForQuad, _s);
                    SetLight(PrimitiveType.Quad, _cyan, new Vector3(_x, -0.033f, 0.387f), RotForQuad, _s);

                    SetLight(PrimitiveType.Quad, _cyan, new Vector3(_x, 0.03f, 0.351f), RotForQuad, _s);
                    SetLight(PrimitiveType.Quad, _cyan, new Vector3(_x, 0, 0.351f), RotForQuad, _s);
                    SetLight(PrimitiveType.Quad, _cyan, new Vector3(_x, -0.033f, 0.351f), RotForQuad, _s);
                    void SetLight(PrimitiveType type, Color color, Vector3 position, Vector3 rotation, Vector3 size)
                    {
                        var obj = new ModelPrimitive(TopServer, type, color, position, rotation, size);
                        Model.AddPart(obj);//*
                        Timing.RunCoroutine(LightBlink(obj, color), "ServerLightBlink");
                    }
                }
                IEnumerator<float> LightBlink(ModelPrimitive part, Color color)
                {
                    for (; ; )
                    {
                        yield return Timing.WaitForSeconds(Random.Range(1, 5));
                        try { part.Primitive.Color = Color.black; } catch { }
                        yield return Timing.WaitForSeconds(Random.Range(1, 5));
                        try { part.Primitive.Color = color; } catch { }
                    }
                }
            }
            #endregion
            #region TopColling
            {
                var TopColling = new Model("TopColling", new Vector3(-0.8246f, 1.076f, 0.02f), Vector3.zero, Model);
                Model.AddPart(new ModelPrimitive(TopColling, PrimitiveType.Cube, serverIn, new Vector3(0.031f, 0, 0), Vector3.zero, new Vector3(0.01f, 0.4f, 0.95f)));
                Model.AddPart(new ModelPrimitive(TopColling, PrimitiveType.Cube, serverIn, new Vector3(-0.038f, 0, 0), Vector3.zero, new Vector3(0.13f, 0.4f, 0.05f)));
                Model.AddPart(new ModelPrimitive(TopColling, PrimitiveType.Cube, serverIn, new Vector3(-0.038f, 0, 0.428f), Vector3.zero, new Vector3(0.13f, 0.4f, 0.1f)));
                Model.AddPart(new ModelPrimitive(TopColling, PrimitiveType.Cube, serverIn, new Vector3(-0.038f, 0, -0.033f), Vector3.zero, new Vector3(0.13f, 0.01f, 0.825f)));
                Model.AddPart(new ModelPrimitive(TopColling, PrimitiveType.Cube, serverIn, new Vector3(-0.038f, 0, -0.4589f), Vector3.zero, new Vector3(0.13f, 0.4f, 0.03f)));
                Model.AddPart(new ModelPrimitive(TopColling, PrimitiveType.Cube, serverIn, new Vector3(-0.038f, -0.1944f, -0.033f), Vector3.zero, new Vector3(0.13f, 0.01f, 0.825f)));
                Model.AddPart(new ModelPrimitive(TopColling, PrimitiveType.Quad, serverIn, new Vector3(-0.06f, 0.008f, -0.032f), RotForQuad, new Vector3(0.85f, 0.4f, 0.01f)));

                Model.AddPart(new ModelPrimitive(TopColling, PrimitiveType.Quad, Color.black, new Vector3(-0.0697f, 0.128f, 0.294f), RotForQuad, new Vector3(0.07f, 0.13f, 0.01f)));
                Model.AddPart(new ModelPrimitive(TopColling, PrimitiveType.Quad, Color.black, new Vector3(-0.0697f, 0.128f, 0.1303f), RotForQuad, new Vector3(0.07f, 0.13f, 0.01f)));
                Model.AddPart(new ModelPrimitive(TopColling, PrimitiveType.Quad, Color.black, new Vector3(-0.0697f, 0.128f, -0.155f), RotForQuad, new Vector3(0.07f, 0.13f, 0.01f)));
                Model.AddPart(new ModelPrimitive(TopColling, PrimitiveType.Quad, Color.black, new Vector3(-0.0697f, 0.128f, -0.326f), RotForQuad, new Vector3(0.07f, 0.13f, 0.01f)));

                Model.AddPart(new ModelPrimitive(TopColling, PrimitiveType.Quad, Color.black, new Vector3(-0.0697f, -0.117f, 0.294f), RotForQuad, new Vector3(0.07f, 0.13f, 0.01f)));
                Model.AddPart(new ModelPrimitive(TopColling, PrimitiveType.Quad, Color.black, new Vector3(-0.0697f, -0.117f, 0.1303f), RotForQuad, new Vector3(0.07f, 0.13f, 0.01f)));
                Model.AddPart(new ModelPrimitive(TopColling, PrimitiveType.Quad, Color.black, new Vector3(-0.0697f, -0.117f, -0.155f), RotForQuad, new Vector3(0.07f, 0.13f, 0.01f)));
                Model.AddPart(new ModelPrimitive(TopColling, PrimitiveType.Quad, Color.black, new Vector3(-0.0697f, -0.117f, -0.326f), RotForQuad, new Vector3(0.07f, 0.13f, 0.01f)));

                Model.AddPart(new ModelPrimitive(TopColling, PrimitiveType.Quad, steklo_black, new Vector3(-0.1014f, 0.005f, -0.032f), RotForQuad, new Vector3(0.83f, 0.4f, 0.01f)));
                Model.AddPart(new ModelPrimitive(TopColling, PrimitiveType.Cylinder, _lime, new Vector3(-0.1033f, -0.0143f, 0.4347f), new Vector3(0, 0, 90), new Vector3(0.01f, 0.001f, 0.01f)));
                Model.AddPart(new ModelPrimitive(TopColling, PrimitiveType.Cylinder, _lime, new Vector3(-0.1033f, 0.1846f, 0.4347f), new Vector3(0, 0, 90), new Vector3(0.01f, 0.001f, 0.01f)));
                CreateDviD(new Vector3(-0.1033f, -0.1585f, 0.4045f), Vector3.zero, TopColling);
                CreateDviD(new Vector3(-0.1033f, 0.0405f, 0.4045f), Vector3.zero, TopColling);
                CreateUsb(new Vector3(-0.1033f, -0.1023f, 0.4045f), Vector3.zero, TopColling);
                CreateUsb(new Vector3(-0.1033f, -0.0627f, 0.4045f), Vector3.zero, TopColling);
                CreateUsb(new Vector3(-0.1033f, 0.0982f, 0.4045f), Vector3.zero, TopColling);
                CreateUsb(new Vector3(-0.1033f, 0.1363f, 0.4045f), Vector3.zero, TopColling);
            }
            #endregion
            #region BottomServers
            {
                Color32 bp = new(0, 57, 81, 255);
                Color32 ps = new(148, 150, 149, 255);
                int _id = 0;
                var BottomServers = new Model("BottomServers", new Vector3(-0.8246f, 0.484f, 0.02f), Vector3.zero, Model);
                Model.AddPart(new ModelPrimitive(BottomServers, PrimitiveType.Quad, new Color32(54, 60, 60, 255), new Vector3(-0.1255f, 0.008f, 0.002f), RotForQuad, new Vector3(0.952f, 0.75f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomServers, PrimitiveType.Quad, ps, new Vector3(-0.131f, 0.008f, -0.47f), new Vector3(0, 90, 90), new Vector3(0.75f, 0.007f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomServers, PrimitiveType.Quad, ps, new Vector3(-0.131f, 0.008f, 0.474f), new Vector3(0, 90, 90), new Vector3(0.75f, 0.007f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomServers, PrimitiveType.Quad, ps, new Vector3(-0.131f, 0.008f, -0.435f), new Vector3(0, 90, 90), new Vector3(0.75f, 0.004f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomServers, PrimitiveType.Quad, ps, new Vector3(-0.131f, 0.008f, 0.44f), new Vector3(0, 90, 90), new Vector3(0.75f, 0.004f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomServers, PrimitiveType.Quad, ps, new Vector3(-0.131f, 0.008f, -0.402f), new Vector3(0, 90, 90), new Vector3(0.75f, 0.004f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomServers, PrimitiveType.Quad, ps, new Vector3(-0.131f, 0.008f, 0.402f), new Vector3(0, 90, 90), new Vector3(0.75f, 0.004f, 0.01f)));

                Model.AddPart(new ModelPrimitive(BottomServers, PrimitiveType.Quad, bp, new Vector3(-0.132f, 0.387f, 0.002f), RotForQuad, new Vector3(0.952f, 0.012f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomServers, PrimitiveType.Quad, bp, new Vector3(-0.132f, 0.24f, 0.002f), RotForQuad, new Vector3(0.952f, 0.025f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomServers, PrimitiveType.Quad, bp, new Vector3(-0.132f, 0.0865f, 0.002f), RotForQuad, new Vector3(0.952f, 0.025f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomServers, PrimitiveType.Quad, bp, new Vector3(-0.132f, -0.0666f, 0.002f), RotForQuad, new Vector3(0.952f, 0.025f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomServers, PrimitiveType.Quad, bp, new Vector3(-0.132f, -0.221f, 0.002f), RotForQuad, new Vector3(0.952f, 0.025f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomServers, PrimitiveType.Quad, bp, new Vector3(-0.132f, -0.3687f, 0.002f), RotForQuad, new Vector3(0.952f, 0.012f, 0.01f)));

                CreateInBottomServers(new Vector3(-0.005f, 0.3181f, 0));
                CreateInBottomServers(new Vector3(-0.005f, 0.1635f, 0));
                CreateInBottomServers(new Vector3(-0.005f, 0.0101f, 0));
                CreateInBottomServers(new Vector3(-0.005f, -0.144f, 0));
                CreateInBottomServers(new Vector3(-0.005f, -0.2989f, 0));
                void CreateInBottomServers(Vector3 pos)
                {
                    _id++;
                    var BottomServer = new Model($"BottomServer #{_id}", pos, Vector3.zero, BottomServers);

                    Model.AddPart(new ModelPrimitive(BottomServer, PrimitiveType.Quad, Color.black, new Vector3(-0.129f, -0.026f, -0.452f), RotForQuad, new Vector3(0.03f, 0.075f, 0.01f)));
                    Model.AddPart(new ModelPrimitive(BottomServer, PrimitiveType.Quad, Color.black, new Vector3(-0.129f, -0.026f, 0.4561f), RotForQuad, new Vector3(0.03f, 0.075f, 0.01f)));
                    Model.AddPart(new ModelPrimitive(BottomServer, PrimitiveType.Quad, _lime, new Vector3(-0.1286f, 0.0558f, -0.4184f), RotForQuad, new Vector3(0.01f, 0.01f, 0.01f)));
                    Model.AddPart(new ModelPrimitive(BottomServer, PrimitiveType.Quad, _yellow, new Vector3(-0.1295f, 0.0417f, 0.4197f), RotForQuad, new Vector3(0.03f, 0.03f, 0.01f)));
                    Model.AddPart(new ModelPrimitive(BottomServer, PrimitiveType.Quad, _red, new Vector3(-0.1295f, 0, 0.4197f), RotForQuad, new Vector3(0.03f, 0.03f, 0.01f)));
                    CreateDviD(new Vector3(-0.1286f, -0.0328621f, -0.4183992f), Vector3.zero, BottomServer);
                    CreateUsb(new Vector3(-0.1286013f, 0.01572892f, -0.4184003f), Vector3.zero, BottomServer);
                    CreateUsb(new Vector3(-0.1286013f, 0.0445f, -0.4184003f), Vector3.zero, BottomServer);

                    Model.AddPart(new ModelPrimitive(BottomServer, PrimitiveType.Quad, Color.black, new Vector3(-0.129f, -0.0224f, 0.155f), RotForQuad, new Vector3(0.48f, 0.06f, 0.01f)));
                    {
                        var obj = new ModelPrimitive(BottomServer, PrimitiveType.Quad, _lime, new Vector3(-0.1291f, 0.0367f, 0.155f), RotForQuad, new Vector3(0.45f, 0.03f, 0.01f));
                        Model.AddPart(obj);//*
                        Timing.RunCoroutine(SectorBlink(obj), "ServerLightBlink");
                    }
                    /*
                    int _sid = 0;
                    CreateSector(new Vector3(-0.1295f, 0.007937878f, 0.383f));
                    CreateSector(new Vector3(-0.1295f, 0.007937878f, 0.3292971f));
                    CreateSector(new Vector3(-0.1295f, 0.007937878f, 0.2747971f));
                    CreateSector(new Vector3(-0.1295f, 0.007937878f, 0.2201971f));
                    CreateSector(new Vector3(-0.1295f, 0.007937878f, 0.1655971f));
                    CreateSector(new Vector3(-0.1295f, 0.007937878f, 0.1112971f));
                    CreateSector(new Vector3(-0.1295f, 0.007937878f, 0.05689707f));
                    CreateSector(new Vector3(-0.1295f, 0.007937878f, 0.00249707f));
                    CreateSector(new Vector3(-0.1295f, 0.007937878f, -0.05140293f));

                    void CreateSector(Vector3 spos)
                    {
                        _sid++;
                        var Sector = new Model($"Sector #{_sid}", spos, Vector3.zero, BottomServer);

                        Model.AddPart(new ModelPrimitive(Sector, PrimitiveType.Quad, Color.black, new Vector3(0.0005f, 0.0093f, -0.01f), RotForQuad, new Vector3(0.05f, 0.06f, 0.01f)));
                        {
                            var obj = new ModelPrimitive(Sector, PrimitiveType.Quad, _lime, new Vector3(0.0004f, 0.0471f, -0.0102f), RotForQuad, new Vector3(0.04f, 0.009f, 0.01f));
                            Model.AddPart(obj);
                            Timing.RunCoroutine(SectorBlink(obj), "ServerLightBlink");
                        }
                    }*/
                }
                IEnumerator<float> SectorBlink(ModelPrimitive part)
                {
                    yield return Timing.WaitForSeconds(Random.Range(0, 20));
                    for (; ; )
                    {
                        yield return Timing.WaitForSeconds(Random.Range(5, 10));
                        try { part.Primitive.Color = _yellow; } catch { }
                        yield return Timing.WaitForSeconds(Random.Range(3, 6));
                        try { part.Primitive.Color = _red; } catch { }
                        yield return Timing.WaitForSeconds(Random.Range(5, 10));
                        try { part.Primitive.Color = _yellow; } catch { }
                        yield return Timing.WaitForSeconds(Random.Range(5, 8));
                        try { part.Primitive.Color = _lime; } catch { }
                        yield return Timing.WaitForSeconds(Random.Range(2, 7));
                    }
                }
            }
            #endregion
            #region BottomColling
            {
                Color32 sbr = new(33, 37, 37, 255);
                var BottomColling = new Model("BottomColling", new Vector3(0, -0.5f, 0.02f), Vector3.zero, Model);

                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Cube, serverIn, new Vector3(-0.85f, 0.185f), Vector3.zero, new Vector3(0.1f, 0.85f, 0.952f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, Color.black, new Vector3(-0.908f, 0.533f, 0.321f), RotForQuad, new Vector3(0.05f, 0.1f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, Color.black, new Vector3(-0.908f, 0.397f, 0.321f), RotForQuad, new Vector3(0.05f, 0.1f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, Color.black, new Vector3(-0.908f, 0.253f, 0.321f), RotForQuad, new Vector3(0.05f, 0.1f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, Color.black, new Vector3(-0.908f, 0.105f, 0.321f), RotForQuad, new Vector3(0.05f, 0.1f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, Color.black, new Vector3(-0.908f, -0.031f, 0.321f), RotForQuad, new Vector3(0.05f, 0.1f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, Color.black, new Vector3(-0.908f, -0.159f, 0.321f), RotForQuad, new Vector3(0.05f, 0.1f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, Color.black, new Vector3(-0.908f, 0.533f, 0.203f), RotForQuad, new Vector3(0.05f, 0.1f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, Color.black, new Vector3(-0.908f, 0.397f, 0.203f), RotForQuad, new Vector3(0.05f, 0.1f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, Color.black, new Vector3(-0.908f, 0.253f, 0.203f), RotForQuad, new Vector3(0.05f, 0.1f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, Color.black, new Vector3(-0.908f, 0.105f, 0.203f), RotForQuad, new Vector3(0.05f, 0.1f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, Color.black, new Vector3(-0.908f, -0.031f, 0.203f), RotForQuad, new Vector3(0.05f, 0.1f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, Color.black, new Vector3(-0.908f, -0.159f, 0.203f), RotForQuad, new Vector3(0.05f, 0.1f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, Color.black, new Vector3(-0.908f, 0.533f, -0.153f), RotForQuad, new Vector3(0.05f, 0.1f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, Color.black, new Vector3(-0.908f, 0.397f, -0.153f), RotForQuad, new Vector3(0.05f, 0.1f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, Color.black, new Vector3(-0.908f, 0.253f, -0.153f), RotForQuad, new Vector3(0.05f, 0.1f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, Color.black, new Vector3(-0.908f, 0.105f, -0.153f), RotForQuad, new Vector3(0.05f, 0.1f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, Color.black, new Vector3(-0.908f, -0.031f, -0.153f), RotForQuad, new Vector3(0.05f, 0.1f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, Color.black, new Vector3(-0.908f, -0.159f, -0.153f), RotForQuad, new Vector3(0.05f, 0.1f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, Color.black, new Vector3(-0.908f, 0.533f, -0.289f), RotForQuad, new Vector3(0.05f, 0.1f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, Color.black, new Vector3(-0.908f, 0.397f, -0.289f), RotForQuad, new Vector3(0.05f, 0.1f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, Color.black, new Vector3(-0.908f, 0.253f, -0.289f), RotForQuad, new Vector3(0.05f, 0.1f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, Color.black, new Vector3(-0.908f, 0.105f, -0.289f), RotForQuad, new Vector3(0.05f, 0.1f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, Color.black, new Vector3(-0.908f, -0.031f, -0.289f), RotForQuad, new Vector3(0.05f, 0.1f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, Color.black, new Vector3(-0.908f, -0.159f, -0.289f), RotForQuad, new Vector3(0.05f, 0.1f, 0.01f)));

                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, _cyan, new Vector3(-0.908f, -0.127f, 0.009f), RotForQuad, new Vector3(0.02f, 0.15f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, _cyan, new Vector3(-0.908f, 0.145f, 0.009f), RotForQuad, new Vector3(0.02f, 0.15f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, _cyan, new Vector3(-0.908f, 0.439f, 0.009f), RotForQuad, new Vector3(0.02f, 0.15f, 0.01f)));

                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Quad, steklo_black, new Vector3(-0.909f, 0.185f), RotForQuad, new Vector3(0.952f, 0.85f, 0.01f)));

                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Cube, sbr, new Vector3(-0.907f, 0.604f), Vector3.zero, new Vector3(0.005f, 0.01f, 0.952f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Cube, sbr, new Vector3(-0.907f, 0.324f), Vector3.zero, new Vector3(0.005f, 0.01f, 0.952f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Cube, sbr, new Vector3(-0.907f, 0.035f), Vector3.zero, new Vector3(0.005f, 0.01f, 0.952f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Cube, sbr, new Vector3(-0.907f, -0.236f), Vector3.zero, new Vector3(0.005f, 0.01f, 0.952f)));

                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Cube, sbr, new Vector3(-0.907f, 0.185f, 0.469f), Vector3.zero, new Vector3(0.005f, 0.85f, 0.01f)));
                Model.AddPart(new ModelPrimitive(BottomColling, PrimitiveType.Cube, sbr, new Vector3(-0.907f, 0.185f, -0.47f), Vector3.zero, new Vector3(0.005f, 0.85f, 0.01f)));
            }
            #endregion
            Timing.CallDelayed(1f, () => Model.Primitives.ForEach(prim =>
            {
                prim.Primitive.MovementSmoothing = 255;
                prim.Primitive.Collider = false;
                prim.Primitive.IsStatic = true;
            }));
        }
        private readonly Vector3 RotForQuad = new(0, 90, 0);
        private void CreateUsb(Vector3 pos, Vector3 rot, Model parent = null)
        {
            var Usb = new Model($"Usb_{pos}", pos, rot, parent);
            Model.AddPart(new ModelPrimitive(Usb, PrimitiveType.Quad, new Color32(143, 148, 154, 255), new Vector3(0, -0.0081f, 0), RotForQuad, new Vector3(0.012f, 0.025f, 0.01f)));
            Model.AddPart(new ModelPrimitive(Usb, PrimitiveType.Quad, Color.black, new Vector3(-0.0001f, -0.0081f, -0.00184f), RotForQuad, new Vector3(0.004f, 0.02f, 0.01f)));
            Model.AddPart(new ModelPrimitive(Usb, PrimitiveType.Quad, new Color(2, 0, 0), new Vector3(-0.0001f, -0.0081f, 0.00216f), RotForQuad, new Vector3(0.004f, 0.0195f, 0.01f)));
        }
        private void CreateDviD(Vector3 pos, Vector3 rot, Model parent = null)
        {
            var DviD = new Model($"DVI-D_{pos}", pos, rot, parent);
            Model.AddPart(new ModelPrimitive(DviD, PrimitiveType.Quad, new Color(1.4f, 1.6f, 1.6f), Vector3.zero, RotForQuad, new Vector3(0.02f, 0.05f, 0.01f)));
            Vector3 _s = new(0.01f, 0.01f, 0.01f);

            Model.AddPart(new ModelPrimitive(DviD, PrimitiveType.Quad, Color.black, new Vector3(-0.0001f, -0.011f, -0.0004f), RotForQuad, _s));
            Model.AddPart(new ModelPrimitive(DviD, PrimitiveType.Quad, Color.black, new Vector3(-0.0001f, 0.0135f, -0.0004f), RotForQuad, _s));
        }
    }
}