// Qurre.API.Addons.Models — движок построек НА РОДНЫХ AdminToys SCP:SL (LabAPI).
// Model = узел-GameObject; ModelPrimitive/Primitive = PrimitiveObjectToy; LightPoint = LightSourceToy.
// Полностью наша реализация — без Qurre/SchematicUnity/основателя.
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

    public class ModelPrimitive : SObject
    {
        public Toys.PrimitiveObjectToy Toy { get; }
        public override dynamic Base => Toy.Base;
        readonly Primitive _primitive;
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
            var parentTr = parent != null ? parent.GameObject.transform : null;
            Toy = Toys.PrimitiveObjectToy.Create(position, Quaternion.Euler(rotation), scale, parentTr, true);
            Toy.Type = type;
            Toy.Color = color;
            Toy.Flags = collidable ? PrimitiveFlags.Collidable | PrimitiveFlags.Visible : PrimitiveFlags.Visible;
            GameObject = Toy.GameObject;
            _primitive = new Primitive(Toy, type);
            parent?.AddPart(this);
        }

        public ModelPrimitive(Model parent, PrimitiveType type, Color color, Vector3 position, Vector3 scale)
            : this(parent, type, color, position, Vector3.zero, scale, false) { }

        public override Color Color { get => Toy.Color; set => Toy.Color = value; }
        public override Vector3 Position { get => Toy.Position; set => Toy.Position = value; }
        public override Vector3 Rotation { get => Toy.Rotation.eulerAngles; set => Toy.Rotation = Quaternion.Euler(value); }
        public override Vector3 Scale { get => Toy.Scale; set => Toy.Scale = value; }

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
            var parentTr = parent != null ? parent.GameObject.transform : null;
            Toy = Toys.LightSourceToy.Create(position, Quaternion.identity, Vector3.one, parentTr, true);
            Toy.Color = color;
            Toy.Intensity = intensity;
            Toy.Range = range;
            Toy.ShadowStrength = shadowStrength;
            GameObject = Toy.GameObject;
            parent?.AddPart(this);
        }

        public override Color Color { get => Toy.Color; set => Toy.Color = value; }
        public override Vector3 Position { get => Toy.Position; set => Toy.Position = value; }
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
            GameObject.transform.localPosition = position;
            GameObject.transform.localEulerAngles = rotation;
            GameObject.transform.localScale = scale;
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
            GameObject.transform.localPosition = position;
            GameObject.transform.localEulerAngles = rotation;
            GameObject.transform.localScale = scale;
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
            GameObject.transform.localPosition = position;
            GameObject.transform.localEulerAngles = rotation;
            GameObject.transform.localScale = scale;
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
            GameObject.transform.localPosition = position;
            GameObject.transform.localEulerAngles = rotation;
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
