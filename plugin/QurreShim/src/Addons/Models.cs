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

        public Model(string name, Vector3 position, Vector3 rotation, Model parent = null)
            : this(name, position, rotation, Vector3.one, parent) { }

        public Model(string name, Vector3 position, Vector3 rotation, Vector3 scale, Model parent = null)
        {
            Name = name;
            GameObject = new GameObject(name);
            if (parent != null)
            {
                Parent = parent;
                GameObject.transform.SetParent(parent.GameObject.transform, false);
                parent.Childrens.Add(this);
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
    }

    public class CustomRoom : Model, IAccessConditions
    {
        public CustomRoom(string name, Vector3 position, Vector3 rotation, Vector3 scale, Model parent = null)
            : base(name, position, rotation, scale, parent) { }
    }

    public class ModelPrimitive : SObject
    {
        public Toys.PrimitiveObjectToy Toy { get; }
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
            parent?.AddPart(this);
        }

        public override Color Color { get => Toy.Color; set => Toy.Color = value; }
        public override Vector3 Position { get => Toy.Position; set => Toy.Position = value; }
        public override Vector3 Rotation { get => Toy.Rotation.eulerAngles; set => Toy.Rotation = Quaternion.Euler(value); }
        public override Vector3 Scale { get => Toy.Scale; set => Toy.Scale = value; }

        public override void Destroy() { try { Toy.Destroy(); } catch { } base.Destroy(); }
    }

    public class Primitive : SObject
    {
        public Toys.PrimitiveObjectToy Toy { get; }
        public PrimitiveType Type { get; }

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
}
