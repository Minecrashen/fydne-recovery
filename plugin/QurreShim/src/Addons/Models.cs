// Qurre.API.Addons.Models — движок построек НА РОДНЫХ AdminToys SCP:SL (LabAPI).
// Model = узел-GameObject; ModelPrimitive/Primitive = PrimitiveObjectToy; LightPoint = LightSourceToy.
// Полностью наша реализация — без Qurre/SchematicUnity/основателя.
//
// ВАЖНО про координаты (выяснено по исходникам LabAPI и поведению AdminToyBase):
// 1. LabAPI AdminToy.Create(position, ..., parent) трактует position/rotation как ЛОКАЛЬНЫЕ
//    относительно parent (setter — transform.localPosition).
// 2. AdminToyBase синхронизирует на клиентов ЛОКАЛЬНЫЕ координаты тоя, а родительские
//    GameObject'ы моделей существуют только на сервере — клиент о них не знает и применяет
//    локальные координаты как мировые.
// Поэтому тои ВСЕГДА создаются без Unity-родителя, сразу в мировых координатах.
// Логическая иерархия (Parent/Childrens) сохраняется, а следование тоев за движущейся
// моделью (двери лифтов и т.п.) обеспечивает ToyAnchorSync на GameObject'е модели.
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AdminToys;
using SchematicUnity.API.Objects;
using Qurre.API.Addons.Audio.Objects;
using Toys = LabApi.Features.Wrappers;

namespace Qurre.API.Addons.Models
{
    public class Model : SObject
    {
        public List<SObject> Objects => Childrens;
        public List<ModelPrimitive> Primitives = new List<ModelPrimitive>();

        public Model(string name, Vector3 position)
            : this(name, position, Vector3.zero, Vector3.one, null) { }

        public Model(string name, Vector3 position, Model root)
            : this(name, position, Vector3.zero, Vector3.one, root) { }

        public Model(string name, Vector3 position, Vector2 rotation)
            : this(name, position, new Vector3(rotation.x, rotation.y, 0f), Vector3.one, null) { }

        public Model(string name, Vector3 position, Vector3 rotation, Model root = null)
            : this(name, position, rotation, Vector3.one, root) { }

        public Model(string name, Vector3 position, Vector3 rotation, Vector3 scale, Model root = null)
        {
            Name = name;
            GameObject = new GameObject(name);
            if (root != null)
            {
                Parent = root;
                GameObject.transform.SetParent(root.GameObject.transform, false);
                root.Childrens.Add(this);
            }
            GameObject.transform.localPosition = position;
            GameObject.transform.localEulerAngles = rotation;
            GameObject.transform.localScale = scale;
        }

        public T AddPart<T>(T obj, bool @static = false) where T : SObject
        {
            if (!Childrens.Contains(obj))
            {
                obj.Parent = this;
                Childrens.Add(obj);
                if (obj is ModelPrimitive mp && !Primitives.Contains(mp)) Primitives.Add(mp);
            }
            return obj;
        }

        public LightPoint AddPart(LightPoint light) => AddPart<LightPoint>(light);
        public SObject AddPart(SObject prim) => AddPart<SObject>(prim);
        public SObject AddPart(SObject prim, bool @static) => AddPart<SObject>(prim, @static);
    }

    public class CustomRoom : Model, IAccessConditions
    {
        public CustomRoom(string name, Vector3 position, Vector3 rotation, Vector3 scale, Model parent = null)
            : base(name, position, rotation, scale, parent) { }
    }

    /// <summary>
    /// Держит непарентованные тои "приклеенными" к серверному GameObject'у модели.
    /// Вешается на GameObject модели; каждый кадр, если модель сдвинулась, пересчитывает
    /// мировые позиции зарегистрированных тоев и проталкивает их в Network*-поля AdminToyBase.
    /// </summary>
    internal sealed class ToyAnchorSync : MonoBehaviour
    {
        internal sealed class Entry
        {
            internal GameObject ToyObject;
            internal object ToyBase;
            internal Vector3 LocalPosition;
            internal Quaternion LocalRotation;
        }

        readonly List<Entry> _entries = new List<Entry>();
        Matrix4x4 _last;
        bool _initialized;

        internal static ToyAnchorSync For(Transform anchor)
        {
            var sync = anchor.GetComponent<ToyAnchorSync>();
            if (sync == null) sync = anchor.gameObject.AddComponent<ToyAnchorSync>();
            return sync;
        }

        internal Entry Register(GameObject toyObject, object toyBase, Vector3 localPosition, Quaternion localRotation)
        {
            var entry = new Entry
            {
                ToyObject = toyObject,
                ToyBase = toyBase,
                LocalPosition = localPosition,
                LocalRotation = localRotation,
            };
            _entries.Add(entry);
            return entry;
        }

        /// <summary>Перезаписать сохранённый локальный оффсет после прямого перемещения тоя в мире.</summary>
        internal void UpdateWorld(Entry entry, Vector3? worldPosition, Quaternion? worldRotation)
        {
            if (entry == null) return;
            if (worldPosition.HasValue) entry.LocalPosition = transform.InverseTransformPoint(worldPosition.Value);
            if (worldRotation.HasValue) entry.LocalRotation = Quaternion.Inverse(transform.rotation) * worldRotation.Value;
        }

        internal static void PushNetworkTransform(object toyBase, Transform toyTransform)
        {
            if (toyBase == null || toyTransform == null) return;
            try
            {
                // Тои без родителя: localPosition == мировой позиции, поэтому Network* всегда консистентны.
                dynamic b = toyBase;
                b.NetworkPosition = toyTransform.localPosition;
                b.NetworkRotation = toyTransform.localRotation;
            }
            catch { }
        }

        void LateUpdate()
        {
            var matrix = transform.localToWorldMatrix;
            if (_initialized && matrix == _last) return;
            _last = matrix;
            _initialized = true;

            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                var entry = _entries[i];
                if (entry.ToyObject == null)
                {
                    _entries.RemoveAt(i);
                    continue;
                }

                var tr = entry.ToyObject.transform;
                tr.SetPositionAndRotation(
                    transform.TransformPoint(entry.LocalPosition),
                    transform.rotation * entry.LocalRotation);
                PushNetworkTransform(entry.ToyBase, tr);
            }
        }
    }

    /// <summary>
    /// Следование непарентованного тоя за произвольным transform'ом (например, за игроком).
    /// Замена прямому `toy.transform.parent = player` — оно ломает сетевую синхронизацию,
    /// потому что AdminToyBase шлёт клиентам локальные координаты, а клиентский той всегда без родителя.
    /// </summary>
    internal sealed class ToyWorldFollow : MonoBehaviour
    {
        internal Transform Target;
        internal Vector3 LocalOffset;
        internal object ToyBase;

        void LateUpdate()
        {
            if (Target == null) return;
            transform.SetPositionAndRotation(Target.TransformPoint(LocalOffset), Target.rotation);
            ToyAnchorSync.PushNetworkTransform(ToyBase, transform);
        }
    }

    public class ModelPrimitive : SObject
    {
        public Toys.PrimitiveObjectToy Toy { get; }
        public override dynamic Base => Toy.Base;
        readonly Primitive _primitive;
        readonly ToyAnchorSync _sync;
        readonly ToyAnchorSync.Entry _anchor;
        public override dynamic Primitive => _primitive;
        public PrimitiveType Type { get; }
        public PrimitiveFlags Flags
        {
            get => Toy.Flags;
            set => Toy.Flags = value;
        }

        public ModelPrimitive(Model parent, PrimitiveType type, Color color, Vector3 position,
                              Vector3 rotation, Vector3 scale, bool collidable = false)
        {
            Type = type;
            var anchorTr = parent != null && parent.GameObject != null ? parent.GameObject.transform : null;
            Quaternion localRotation = Quaternion.Euler(rotation);
            Vector3 worldPosition = anchorTr != null ? anchorTr.TransformPoint(position) : position;
            Quaternion worldRotation = anchorTr != null ? anchorTr.rotation * localRotation : localRotation;
            // Без Unity-родителя: клиенты применяют синхронизированные локальные координаты как мировые.
            Toy = Toys.PrimitiveObjectToy.Create(worldPosition, worldRotation, scale, null, true);
            Toy.Type = type;
            Toy.Color = color;
            Toy.Flags = collidable ? PrimitiveFlags.Collidable | PrimitiveFlags.Visible : PrimitiveFlags.Visible;
            GameObject = Toy.GameObject;
            _primitive = new Primitive(Toy, type);
            if (anchorTr != null)
            {
                _sync = ToyAnchorSync.For(anchorTr);
                _anchor = _sync.Register(GameObject, (object)Toy.Base, position, localRotation);
            }
            parent?.AddPart(this);
        }

        public ModelPrimitive(Model parent, PrimitiveType type, Color color, Vector3 position, Vector3 scale)
            : this(parent, type, color, position, Vector3.zero, scale, false) { }

        public override Color Color { get => Toy.Color; set => Toy.Color = value; }
        public override Vector3 Position
        {
            get => GameObject != null ? GameObject.transform.position : Vector3.zero;
            set
            {
                if (GameObject == null) return;
                GameObject.transform.position = value;
                if (_sync != null) _sync.UpdateWorld(_anchor, value, null);
                ToyAnchorSync.PushNetworkTransform((object)Toy.Base, GameObject.transform);
            }
        }
        public override Vector3 Rotation
        {
            get => GameObject != null ? GameObject.transform.eulerAngles : Vector3.zero;
            set
            {
                if (GameObject == null) return;
                Quaternion rotation = Quaternion.Euler(value);
                GameObject.transform.rotation = rotation;
                if (_sync != null) _sync.UpdateWorld(_anchor, null, rotation);
                ToyAnchorSync.PushNetworkTransform((object)Toy.Base, GameObject.transform);
            }
        }
        public override Vector3 Scale
        {
            get => Toy.Scale;
            set
            {
                Toy.Scale = value;
                try { ((dynamic)(object)Toy.Base).NetworkScale = value; } catch { }
            }
        }

        public override void Destroy() { try { Toy.Destroy(); } catch { } base.Destroy(); }
    }

    public class Primitive : SObject
    {
        public Toys.PrimitiveObjectToy Toy { get; }
        public override dynamic Base => Toy.Base;
        public PrimitiveType Type { get; }

        internal Primitive(Toys.PrimitiveObjectToy toy, PrimitiveType type) : base(false)
        {
            Toy = toy;
            Type = type;
            GameObject = toy.GameObject;
        }

        public Primitive(PrimitiveType type, Vector3 position, Color color,
                         Vector3 size = default, Vector3 rotation = default, bool collidable = false)
        {
            Type = type;
            if (size == default) size = Vector3.one;
            Toy = Toys.PrimitiveObjectToy.Create(position, Quaternion.Euler(rotation), size, null, true);
            Toy.Type = type;
            Toy.Color = color;
            Toy.Flags = collidable ? PrimitiveFlags.Collidable | PrimitiveFlags.Visible : PrimitiveFlags.Visible;
            GameObject = Toy.GameObject;
        }

        public override Color Color { get => Toy.Color; set => Toy.Color = value; }
        public override Vector3 Position { get => Toy.Position; set => Toy.Position = value; }
        public override void Destroy() { try { Toy.Destroy(); } catch { } base.Destroy(); }
    }

    public class LightPoint : SObject
    {
        public Toys.LightSourceToy Toy { get; }
        readonly ToyAnchorSync _sync;
        readonly ToyAnchorSync.Entry _anchor;
        public override dynamic Base => Toy.Base;
        public override dynamic Light => Toy.Base;
        public float ShadowStrength { get => Toy.ShadowStrength; set => Toy.ShadowStrength = value; }

        public LightPoint(Vector3 position, Color color, float intensity, float range, float shadowStrength = 0f)
        {
            Toy = Toys.LightSourceToy.Create(position, Quaternion.identity, Vector3.one, null, true);
            Toy.Color = color;
            Toy.Intensity = intensity;
            Toy.Range = range;
            Toy.ShadowStrength = shadowStrength;
            GameObject = Toy.GameObject;
        }

        public LightPoint(Model parent, Color color, Vector3 position, float intensity, float range, float shadowStrength = 0f)
        {
            var anchorTr = parent != null && parent.GameObject != null ? parent.GameObject.transform : null;
            Vector3 worldPosition = anchorTr != null ? anchorTr.TransformPoint(position) : position;
            // Как и примитивы: без Unity-родителя, мировые координаты, следование через ToyAnchorSync.
            Toy = Toys.LightSourceToy.Create(worldPosition, Quaternion.identity, Vector3.one, null, true);
            Toy.Color = color;
            Toy.Intensity = intensity;
            Toy.Range = range;
            Toy.ShadowStrength = shadowStrength;
            GameObject = Toy.GameObject;
            if (anchorTr != null)
            {
                _sync = ToyAnchorSync.For(anchorTr);
                _anchor = _sync.Register(GameObject, (object)Toy.Base, position, Quaternion.identity);
            }
            parent?.AddPart(this);
        }

        public override Color Color { get => Toy.Color; set => Toy.Color = value; }
        public override Vector3 Position
        {
            get => GameObject != null ? GameObject.transform.position : Vector3.zero;
            set
            {
                if (GameObject == null) return;
                GameObject.transform.position = value;
                if (_sync != null) _sync.UpdateWorld(_anchor, value, null);
                ToyAnchorSync.PushNetworkTransform((object)Toy.Base, GameObject.transform);
            }
        }

        /// <summary>Следовать за transform'ом (игрок/камера) с локальным оффсетом, без Unity-парентинга.</summary>
        public void Follow(Transform target, Vector3 localOffset)
        {
            if (GameObject == null) return;
            if (GameObject.transform.parent != null) GameObject.transform.SetParent(null, true);
            var follow = GameObject.GetComponent<ToyWorldFollow>();
            if (follow == null) follow = GameObject.AddComponent<ToyWorldFollow>();
            follow.Target = target;
            follow.LocalOffset = localOffset;
            follow.ToyBase = (object)Toy.Base;
        }

        public void StopFollow()
        {
            var follow = GameObject != null ? GameObject.GetComponent<ToyWorldFollow>() : null;
            if (follow != null) follow.Target = null;
        }

        public override void Destroy() { try { Toy.Destroy(); } catch { } base.Destroy(); }
    }

    public class ModelLight : LightPoint
    {
        public ModelLight(Model parent, Color color, Vector3 position, float lightIntensity = 1f, float lightRange = 5f, float shadowStrength = 0f)
            : base(parent, color, position, lightIntensity, lightRange, shadowStrength) { }
    }

    public class ModelTarget : SObject
    {
        public ModelTarget(Model parent, object prefab, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            Name = "Target";
            GameObject = new GameObject(Name);
            AttachLocal(GameObject, parent, position, rotation, scale);
            parent?.AddPart(this);
        }
    }

    public class ModelWorkStation : SObject
    {
        public Qurre.API.Controllers.WorkStation WorkStation { get; }
        public ModelWorkStation(Model parent, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            Name = "WorkStation";
            GameObject = new GameObject(Name);
            AttachLocal(GameObject, parent, position, rotation, scale);
            WorkStation = Qurre.API.Controllers.WorkStation.Get(GameObject.transform);
            parent?.AddPart(this);
        }
    }

    public class ModelDoor : SObject
    {
        public ModelDoor(Model parent, object prefab, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            Name = "Door";
            GameObject = new GameObject(Name);
            AttachLocal(GameObject, parent, position, rotation, scale);
            parent?.AddPart(this);
        }
    }

    public class ModelPickup : SObject
    {
        public PickupData Pickup { get; } = new PickupData();

        public ModelPickup() { }

        public ModelPickup(Model parent, ItemType item, Vector3 position, Vector3 rotation, bool kinematic = true)
        {
            Name = item.ToString();
            GameObject = new GameObject(Name);
            AttachLocal(GameObject, parent, position, rotation, Vector3.one);
            Pickup.ItemType = item;
            parent?.AddPart(this);
        }

        public class PickupData
        {
            static ushort _next = 1;
            public ushort Serial { get; set; } = _next++;
            public ItemType ItemType { get; set; }
        }
    }
}
