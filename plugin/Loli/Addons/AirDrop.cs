using Loli.Controllers;
using MEC;
using Qurre.API;
using Qurre.API.Addons.Models;
using Qurre.API.Attributes;
using Qurre.Events;
using System.Collections.Generic;
using LabApi.Features.Wrappers;
using UnityEngine;
using Round = Qurre.API.World.Round;

namespace Loli.Addons
{
    static class AirDrop
    {
        #region Coroutine
        [EventMethod(RoundEvents.Start)]
        static void RunAirDropCoroutine() => Timing.RunCoroutine(AirDropThread(), "AirDropThread");

        [EventMethod(RoundEvents.End)]
        static void KillAirDropCoroutine() => Timing.KillCoroutines("AirDropThread");

        static IEnumerator<float> AirDropThread()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(200);
                if (Round.Started)
                {
                    Call();
                }
            }
        }
        #endregion

        static Model PlaneModel;
        static bool _calling = false;

        [EventMethod(RoundEvents.Waiting)]
        static internal void Waiting()
        {
            _calling = false;
            PlaneModel = new("AirDrope_Plane", new(450, 1031, -42f), new(0, 180));
            PlaneModel.AddPart(new ModelPrimitive(PlaneModel, PrimitiveType.Quad, new Color(4, 0, 0), Vector3.zero, new(90, 0, 0), new(10, 3, 1), false));
            PlaneModel.AddPart(new ModelPrimitive(PlaneModel, PrimitiveType.Quad, new Color(4, 0, 0), new(0, 0, 2.3f), new(90, -60, 0), new(2, 7, 1), false));
            PlaneModel.AddPart(new ModelPrimitive(PlaneModel, PrimitiveType.Quad, new Color(4, 0, 0), new(0, 0, -2.3f), new(90, 60, 0), new(2, 7, 1), false));
            PlaneModel.AddPart(new ModelPrimitive(PlaneModel, PrimitiveType.Quad, new Color(4, 0, 0), new(-6, 0, 0), new(90, 0, 0), new(2, 1, 1), false));
            PlaneModel.AddPart(new ModelPrimitive(PlaneModel, PrimitiveType.Quad, new Color(4, 0, 0), new(6, 0, 0), new(90, 0, 0), new(2, 2, 1), false));
            PlaneModel.Primitives.ForEach(prim =>
            {
                prim.Primitive.MovementSmoothing = 64;
                prim.GameObject.name = "AirDrop";
            });
            PlaneModel.GameObject.AddComponent<FixPrimitiveSmoothing>().Model = PlaneModel;
        }
        static internal void Call()
        {
#if NR
            if (Commands.EventMode)
                return;
#endif

            if (ConceptsController.IsActivated)
                return;

            if (_calling)
                return;

            PlaneModel.GameObject.transform.position = new(450, 1031, -42);
            _calling = true;//130  120  110  -8  -110  -120  -130
            float _dropLoc = Random.Range(0, 700) switch
            {
                >= 550 => 130,
                >= 400 => 120,
                >= 250 => 110,
                >= 100 => -8,
                >= 50 => -110,
                >= 30 => -120,
                _ => -130
            };
            Timing.RunCoroutine(DoRun());
            IEnumerator<float> DoRun()
            {
                bool dropped = false;
                for (; ; )
                {
                    Vector3 pos = PlaneModel.GameObject.transform.position;
                    PlaneModel.GameObject.transform.position = new(pos.x - 3f, pos.y, pos.z);
                    if (!dropped && pos.x.Difference(_dropLoc) < 3.1f)
                    {
                        dropped = true;
                        new Drop(pos);
                    }
                    if (pos.x < -450)
                    {
                        PlaneModel.GameObject.transform.position = new(450, pos.y, pos.z);
                        _calling = false;
                        yield break;
                    }
                    yield return Timing.WaitForSeconds(0.1f);
                }
            }
        }
        static readonly List<DropItem> PossibleItems = new()
        {
            new(ItemType.GunE11SR, 1),
            new(ItemType.Ammo556x45, 7),
            new(ItemType.Ammo9x19, 7),
            new(ItemType.Adrenaline, 4),
            new(ItemType.Medkit, 3),
            new(ItemType.GrenadeFlash, 2),
            new(ItemType.GrenadeHE, 2)
        };
        public struct DropItem
        {
            public ItemType Type;
            public int Amout;
            public DropItem(ItemType type, int amout)
            {
                Type = type;
                Amout = amout;
            }
        }
        public class Drop
        {
            private readonly Model _model;
            private readonly List<GameObject> _faces = new();
            private readonly GameObject _balloon;

            public Drop(Vector3 position)
            {
                _model = new("Drop", position);
                for (float i = -0.5f; i < 1; i++)
                {
                    _faces.Add(new Face(new Vector3(i, 0, 0), Vector3.up * 90, _model).Model.GameObject);
                    _faces.Add(new Face(new Vector3(0, i, 0), Vector3.right * 90, _model).Model.GameObject);
                    _faces.Add(new Face(new Vector3(0, 0, i), Vector3.zero, _model).Model.GameObject);
                }

                var rndColor = Random.ColorHSV();
                {
                    var _prim = new ModelPrimitive(_model, PrimitiveType.Sphere, rndColor, Vector3.up * 2.125f, Vector3.zero, Vector3.one * -2);
                    _prim.GameObject.name = "AirDrop";
                    _prim.Primitive.MovementSmoothing = 64;
                    _prim.GameObject.AddComponent<FixOnePrimitiveSmoothing>().Primitive = _prim.Primitive;
                    _balloon = _prim.GameObject;
                }
                Model _bm = new("Light", _balloon.transform.position, _balloon.transform.rotation.eulerAngles);
                new ModelLight(_bm, rndColor, Vector3.zero);

                var controller = _model.GameObject.AddComponent<DropController>();
                controller.balloon = _balloon;
                controller.faces = _faces;
            }

            public class BalloonController : MonoBehaviour
            {
                private float _startPos;

                private void Start() => _startPos = transform.position.y;

                private void Update()
                {
                    if (transform.position.y - _startPos < 15)
                    {
                        transform.position += Vector3.up * Time.deltaTime * 10;
                        transform.localScale += Vector3.one * Time.deltaTime * 1.25f;
                    }
                    else Destroy(gameObject);
                }
            }
            public class DropController : MonoBehaviour
            {
                private Rigidbody _rigidbody;
                private bool _collided;
                private bool _crateOpened;

                public GameObject balloon;
                public List<GameObject> faces;

                private void Start()
                {
                    ChangeLayers(transform, 6);

                    _rigidbody = gameObject.AddComponent<Rigidbody>();
                    _rigidbody.mass = 20;
                    _rigidbody.drag = 3;
                    _rigidbody.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
                }

                private void Update()
                {
                    if (!_collided)
                        transform.localEulerAngles += Vector3.up * Time.deltaTime * 30;
                }

                private void OnCollisionEnter(Collision _)
                {
                    if (_.gameObject.name == "AirDrop") return;
                    var __p1 = _.gameObject.transform.position;
                    var __p2 = PlaneModel.GameObject.transform.position;
                    if (__p1.y == __p2.y && __p1.z == __p2.z) return;

                    if (_collided) return;

                    if (_.gameObject.name.Contains("Don't call it a grave, it's the future you chose"))
                    {
                        transform.position += Vector3.down * 0.3f;
                        return;
                    }

                    if (_.gameObject.name.Contains("Collision Box"))
                    {
                        transform.position += Vector3.down * 0.3f;
                        return;
                    }

                    _collided = true;
                    Destroy(gameObject.GetComponent<Rigidbody>());

                    balloon.AddComponent<BalloonController>();

                    AddTrigger();
                }

                private void AddTrigger()
                {
                    var collider = gameObject.AddComponent<BoxCollider>();
                    collider.isTrigger = true;
                    collider.size = Vector3.one * 5f;
                    collider.center = Vector3.up;
                }

                private void OnTriggerEnter(Collider other)
                {
                    if (!_collided || _crateOpened || (!other.gameObject.name.Contains("Player") &&
                        !other.gameObject.name.Contains("Scp018Projectile") && !other.gameObject.name.Contains("HegProjectile")))
                        return;

                    _crateOpened = true;

                    var _itm = PossibleItems[Random.Range(0, PossibleItems.Count)];
                    for (int i = 0; i < _itm.Amout; i++)
                        Pickup.Create(_itm.Type, transform.position);

                    foreach (var face in faces)
                    {
                        var r = face.AddComponent<Rigidbody>();
                        r.AddExplosionForce(20, transform.position, 1);
                    }

                    Timing.CallDelayed(5, () => Destroy(gameObject));
                }

                private static void ChangeLayers(Transform t, int layer)
                {
                    t.gameObject.layer = layer;
                    foreach (Transform child in t)
                    {
                        ChangeLayers(child, layer);
                    }
                }
            }

            private class Face
            {
                public Model Model;

                public Face(Vector3 pos, Vector3 rot, Model drop)
                {
                    Model = new("Fave", pos, rot, drop);

                    var scale = new Vector3(1, 0.2f, 0.125f);

                    for (float i = -0.5f; i < 1; i++)
                    {
                        Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, Color.black, new Vector3(i, 0, 0), Vector3.forward * 90, scale));
                        Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, Color.black, new Vector3(0, i, 0), Vector3.zero, scale));
                    }

                    Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, Color.gray, Vector3.zero, Vector3.zero, new Vector3(1, 1, 0.1f)));
                    Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, Color.black, Vector3.zero, Vector3.forward * 45, new Vector3(1.2f, 0.2f, 0.125f)));

                    foreach (var child in Model.Primitives)
                    {
                        child.GameObject.name = "AirDrop";
                        child.Primitive.MovementSmoothing = 64;
                        child.GameObject.AddComponent<FixOnePrimitiveSmoothing>().Primitive = child.Primitive;
                    }
                }
            }
        }
    }
}