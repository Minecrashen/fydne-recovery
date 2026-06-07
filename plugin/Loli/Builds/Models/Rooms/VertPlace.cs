using Loli.Addons;
using MEC;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Addons.Models;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using SchematicUnity.API.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using Qurre.API.Controllers;
using UnityEngine;

namespace Loli.Builds.Models.Rooms
{
    static class VertPlace
    {
        const string CoroutineName = "VertPlace_CallHelicopter";

        static Scheme helicopter;
        static PropellerX propellerX;
        static Ventilator ventilator;
        static readonly List<VertDoor> doors = new();
        static HelicopterStatus helicopterStatus;


        [EventMethod(RoundEvents.Waiting)]
        static void Load()
        {
            doors.Clear();
            propellerX = null;
            ventilator = null;
            helicopterStatus = HelicopterStatus.Sleeping;

            var scheme = SchematicUnity.API.SchematicManager.LoadSchematic(Path.Combine(Paths.Plugins, "Schemes", "VertPlace.json"), new(0, -700));

            foreach (var _obj in scheme.Objects)
                FindObjectsVertPlace(_obj);

            new Radar(new(135.87f, 306.4f, -71.92f), Vector3.zero);

            helicopter = SchematicUnity.API.SchematicManager.LoadSchematic(Path.Combine(Paths.Plugins, "Schemes", "Helicopter.json"), new(50f, 286.2f, -185f));

            foreach (var _obj in helicopter.Objects)
                FindObjectsHelicopter(_obj);

        }

        static void Event(InteractWorkStationEvent ev)
        {
            ev.Allowed = false;

#if MRP
            if (!ev.Player.RoleInformation.IsHuman)
                return;
#endif

            if (helicopterStatus == HelicopterStatus.FlyingTo)
            {
                ev.Player.Client.ShowHint("<color=#4865fc>Эвакуационный вертолет уже вызван</color>", 10);
                return;
            }
            if (helicopterStatus == HelicopterStatus.FlyingFrom)
            {
                ev.Player.Client.ShowHint("<color=#ff7073>Эвакуационный вертолет совсем недавно вылетел из комплекса</color>", 10);
                return;
            }

            if (helicopterStatus == HelicopterStatus.Waiting)
            {
                ev.Player.Client.ShowHint("<color=#f47fff>Эвакуационный вертолет успешно отправлен из комплекса</color>", 10);

                Timing.KillCoroutines(CoroutineName);
                Timing.RunCoroutine(UncallHelicopter(), CoroutineName);

                return;
            }

            if (helicopterStatus == HelicopterStatus.Sleeping)
            {
                ev.Player.Client.ShowHint("<color=#58db5c>Эвакуационный вертолет успешно вызван</color>", 10);

                Timing.KillCoroutines(CoroutineName);
                Timing.RunCoroutine(CallHelicopter(), CoroutineName);

                return;
            }

            ev.Player.Client.ShowHint("<color=#ff3e0d>Статус эвакуационного вертолета неизвествен</color>", 10);
        }


        static void FindObjectsVertPlace(SObject obj)
        {
            if (obj is null)
                return;

            if (obj.Primitive != null)
            {
                PrimitiveParams prm = (PrimitiveParams)obj.Primitive;
                prm.Base.IsStatic = true;
            }

            if (obj.Name == "ForWork" && obj.Primitive != null)
            {
                PrimitiveParams prm = (PrimitiveParams)obj.Primitive;
                prm.Base.Flags = 0;
                Model work = new("ForWork", Vector3.zero);
                work.GameObject.transform.position = prm.Position;
                work.GameObject.transform.localRotation = prm.Rotation;
                work.GameObject.transform.localScale = prm.Scale;

                ModelWorkStation station = new(work, Vector3.zero, Vector3.zero, Vector3.one);
                StationsManager.Register(station.WorkStation, Event);

                work.AddPart(station);
            }
            else if (obj.Name == "VisualPrim" && obj.Primitive != null)
            {
                PrimitiveParams prm = (PrimitiveParams)obj.Primitive;
                prm.Base.Collider = false;
            }
            else if (obj.Light != null)
            {
                LightParams light = (LightParams)obj.Light;
                light.Shadows = false;
            }

            if (obj.Childrens is null)
                return;

            foreach (var _obj in obj.Childrens)
                FindObjectsVertPlace(_obj);
        }

        static void FindObjectsHelicopter(SObject obj)
        {
            if (obj is null)
                return;

            if (obj.Primitive != null)
            {
                PrimitiveParams prm = (PrimitiveParams)obj.Primitive;
                prm.Base.MovementSmoothing = 64;
                prm.Base.IsStatic = true;
            }

            if (obj.Light != null)
            {
                LightParams lgh = (LightParams)obj.Light;
                lgh.Shadows = false;
            }

            if (obj.Name == "PropellerX")
            {
                propellerX = obj.Transform.gameObject.AddComponent<PropellerX>();
            }

            else if (obj.Name == "Ventilator")
            {
                ventilator = obj.Transform.gameObject.AddComponent<Ventilator>();
            }

            else if (obj.Name == "Door")
            {
                doors.Add(new VertDoor(obj));
            }

            else if (obj.Name == "Escape" && obj.Primitive != null)
            {
                PrimitiveParams prm = (PrimitiveParams)obj.Primitive;
                prm.Base.Base.gameObject.AddComponent<EscapeScript>();
            }

            if (obj.Childrens is null)
                return;

            foreach (var _obj in obj.Childrens)
                FindObjectsHelicopter(_obj);
        }

        static void SetHelicopterStatic(bool @static)
        {
            if (helicopter.Objects is null)
                return;

            foreach (var _obj in helicopter.Objects)
                FindObjects(_obj, @static);

            static void FindObjects(SObject obj, bool @static)
            {
                if (obj is null)
                    return;

                if (obj.Primitive != null)
                {
                    PrimitiveParams prm = (PrimitiveParams)obj.Primitive;
                    prm.Base.IsStatic = @static;
                }

                if (obj.Childrens is null)
                    return;

                foreach (var _obj in obj.Childrens)
                    FindObjects(_obj, @static);
            }
        }


        // -------- call --------
        // 0: pos[293f, 1000.2f, -183f] rot[0, 132] -> 15s
        // 1: pos[136f, 1014.2f, -58f] rot[0, 132] -> 5s
        // 2: pos[136f, 1001.2f, -58f] rot[0, 328]
        // -------- call --------

        // ------- uncall -------
        // 0: pos[136f, 1001.2f, -58f] rot[0, 328] -> 5s
        // 1: pos[136f, 1019.2f, -58f] rot[0, 394] -> 15s
        // 2: pos[50f, 1013.2f, -185f] rot[0, 394] -> 0s // vert off
        // 3: pos[50f, 986.2f, -185f] rot[0, 394]
        // ------- uncall -------

        static internal IEnumerator<float> CallHelicopter()
        {
            propellerX.SetStatus(true);
            ventilator.SetStatus(true);
            SetHelicopterStatic(false);

            Vector3 pos0 = new(293f, 300.2f, -183f);
            Vector3 rot0 = new(0, 132);
            Vector3 pos1 = new(136f, 314.2f, -58f);
            Vector3 pos2 = new(136f, 301.2f, -58f);
            Vector3 rot2 = new(0, 328);

            Vector3 step0 = (pos1 - pos0) / 300; // 15 / 0.05
            Vector3 step1 = (pos2 - pos1) / 100; // 5 / 0.05
            Vector3 stepRot1 = (rot2 - rot0) / 100; // 5 / 0.05


            helicopterStatus = HelicopterStatus.FlyingTo;

            helicopter.Transform.position = pos0;
            helicopter.Transform.rotation = Quaternion.Euler(rot0);

            for (float i = 0; i < 15; i += 0.05f)
            {
                helicopter.Transform.position += step0;
                yield return Timing.WaitForSeconds(0.03f);
            }

            helicopter.Transform.position = pos1;

            yield return Timing.WaitForSeconds(1f);

            for (float i = 0; i < 5; i += 0.05f)
            {
                helicopter.Transform.position += step1;
                helicopter.Transform.rotation = Quaternion.Euler(rot0 + (stepRot1 * i * 20));
                yield return Timing.WaitForSeconds(0.05f);
            }

            helicopter.Transform.position = pos2;
            helicopter.Transform.rotation = Quaternion.Euler(rot2);

            yield return Timing.WaitForSeconds(1f);

            propellerX.SetStatus(false);
            ventilator.SetStatus(false);

            foreach (var door in doors)
                door.Transform.localPosition = door.StartedPosition;

            for (float i = 0; i < 10; i += 0.5f)
            {
                foreach (var door in doors)
                    door.Transform.localPosition += Vector3.back * 0.05f;

                yield return Timing.WaitForSeconds(0.05f);
            }

            yield return Timing.WaitForSeconds(5f);

            helicopterStatus = HelicopterStatus.Waiting;
            SetHelicopterStatic(true);

            yield return Timing.WaitForSeconds(45f);

            Timing.RunCoroutine(UncallHelicopter(), CoroutineName);

            yield break;
        }

        static internal IEnumerator<float> UncallHelicopter()
        {
            Vector3 pos0 = new(136f, 301.2f, -58f);
            Vector3 rot0 = new(0, 328);
            Vector3 pos1 = new(136f, 319.2f, -58f);
            Vector3 rot1 = new(0, 394);
            Vector3 pos2 = new(50f, 313.2f, -185f);
            Vector3 pos3 = new(50f, 286.2f, -185f);

            Vector3 step0 = (pos1 - pos0) / 100; // 5 / 0.05
            Vector3 stepRot0 = (rot1 - rot0) / 100; // 5 / 0.05
            Vector3 step1 = (pos2 - pos1) / 300; // 15 / 0.05


            helicopterStatus = HelicopterStatus.FlyingFrom;
            SetHelicopterStatic(false);

            helicopter.Transform.position = pos0;
            helicopter.Transform.rotation = Quaternion.Euler(rot0);

            for (float i = 0; i < 10; i += 0.5f)
            {
                foreach (var door in doors)
                    door.Transform.localPosition += Vector3.forward * 0.05f;

                yield return Timing.WaitForSeconds(0.05f);
            }

            foreach (var door in doors)
                door.Transform.localPosition = door.StartedPosition;

            propellerX.SetStatus(true);
            ventilator.SetStatus(true);

            yield return Timing.WaitForSeconds(1f);

            for (float i = 0; i < 5; i += 0.05f)
            {
                helicopter.Transform.position += step0;
                helicopter.Transform.rotation = Quaternion.Euler(rot0 + (stepRot0 * i * 20));
                yield return Timing.WaitForSeconds(0.05f);
            }

            helicopter.Transform.position = pos1;
            helicopter.Transform.rotation = Quaternion.Euler(rot1);

            yield return Timing.WaitForSeconds(1f);

            for (float i = 0; i < 15; i += 0.05f)
            {
                helicopter.Transform.position += step1;
                yield return Timing.WaitForSeconds(0.03f);
            }

            helicopter.Transform.position = pos2;

            yield return Timing.WaitForSeconds(1f);

            helicopter.Transform.position = pos3;

            propellerX.SetStatus(false);
            ventilator.SetStatus(false);
            SetHelicopterStatic(true);

            yield return Timing.WaitForSeconds(5f);

            helicopterStatus = HelicopterStatus.Sleeping;

            yield break;
        }

        class VertDoor
        {
            internal Transform Transform { get; }
            internal Vector3 StartedPosition { get; }

            internal VertDoor(SObject obj)
            {
                Transform = obj.Transform;
                StartedPosition = obj.Transform.localPosition;
            }
        }

        class PropellerX : MonoBehaviour
        {
            bool _enabled = false;
            float _nextCycle = 0f;
            readonly float _interval = 0.035f;
            Vector3 _startRot;

            internal void SetStatus(bool enbl)
                => _enabled = enbl;

            void Start()
            {
                _startRot = transform.localRotation.eulerAngles;
            }

            void Update()
            {
                if (!_enabled)
                    return;

                if (Time.time < _nextCycle)
                    return;

                _nextCycle += _interval;

                _startRot.x += 20;
                if (_startRot.x >= 360)
                    _startRot.x = 0;

                transform.localRotation = Quaternion.Euler(_startRot);
            }
        }

        class Ventilator : MonoBehaviour
        {
            bool _enabled = false;
            float _nextCycle = 0f;
            readonly float _interval = 0.01f;

            internal void SetStatus(bool enbl)
                => _enabled = enbl;

            void Update()
            {
                if (!_enabled)
                    return;

                if (Time.time < _nextCycle)
                    return;

                _nextCycle += _interval;

                Vector3 rot = transform.localRotation.eulerAngles;
                transform.localRotation = Quaternion.Euler(new(rot.x, rot.y + 20, rot.z));
            }
        }

        class EscapeScript : MonoBehaviour
        {
            private void Start()
            {
                var collider = gameObject.AddComponent<BoxCollider>();
                collider.isTrigger = true;
                collider.size = new(1.5f, 22, 1.5f);
                collider.center = Vector3.up * 5;
            }

            private void OnTriggerEnter(Collider other)
            {
                if (!other.gameObject.name.Contains("Player"))
                    return;

                Player pl = other.gameObject.GetPlayer();

                if (!pl.RoleInformation.IsAlive)
                    return;

                if ((DateTime.Now - pl.SpawnedTime).TotalSeconds < 10f)
                    return;

                pl.Inventory.Clear();
                pl.InvokeEscape(RoleTypeId.None);
                pl.RoleInformation.SetNew(RoleTypeId.Spectator, RoleChangeReason.Escaped);
            }
        }

        enum HelicopterStatus
        {
            FlyingTo,
            FlyingFrom,
            Waiting,
            Sleeping,
        }
    }
}