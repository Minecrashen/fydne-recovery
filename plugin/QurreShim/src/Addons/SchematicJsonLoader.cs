using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Qurre.API.Addons.Models;
using SchematicUnity.API.Objects;
using UnityEngine;

namespace SchematicUnity.API
{
    internal static class SchematicJsonLoader
    {
        public static Scheme Load(string path, Vector3 position, Quaternion rotation)
        {
            var scheme = new Scheme
            {
                Name = Path.GetFileNameWithoutExtension(path)
            };
            scheme.Position = position;
            scheme.Rotation = rotation.eulerAngles;

            if (!File.Exists(path)) return scheme;

            try
            {
                var root = JToken.Parse(File.ReadAllText(path));
                foreach (var node in Objects(root))
                {
                    var objectKind = ReadString(node, "ObjectType", "objectType", "Type", "type", "Name", "name");
                    if (!LooksLikeSpawnObject(node, objectKind)) continue;

                    var localPosition = ReadVector(node, Vector3.zero, "Position", "position", "LocalPosition", "localPosition");
                    var localRotation = ReadVector(node, Vector3.zero, "Rotation", "rotation", "LocalRotation", "localRotation");
                    var localScale = ReadVector(node, Vector3.one, "Scale", "scale", "Size", "size");
                    var worldPosition = position + rotation * localPosition;
                    var worldRotation = (rotation * Quaternion.Euler(localRotation)).eulerAngles;

                    SObject obj = null;
                    if (IsLight(objectKind, node))
                    {
                        obj = new LightPoint(
                            worldPosition,
                            ReadColor(node, Color.white),
                            ReadFloat(node, 1f, "Intensity", "intensity", "LightIntensity", "lightIntensity"),
                            ReadFloat(node, 5f, "Range", "range", "LightRange", "lightRange"),
                            ReadFloat(node, 0f, "ShadowStrength", "shadowStrength"));
                    }
                    else if (TryReadPrimitiveType(node, out var primitiveType))
                    {
                        obj = new Primitive(
                            primitiveType,
                            worldPosition,
                            ReadColor(node, Color.white),
                            localScale,
                            worldRotation,
                            ReadBool(node, false, "Collidable", "collidable", "Collider", "collider"));
                    }

                    Attach(scheme, obj);
                }
            }
            catch
            {
            }

            return scheme;
        }

        static bool LooksLikeSpawnObject(JObject node, string objectKind)
            => HasAny(node, "Position", "position", "LocalPosition", "localPosition")
               && (IsLight(objectKind, node) ||
                   HasAny(node, "PrimitiveType", "primitiveType", "Primitive", "primitive", "Shape", "shape"));

        static System.Collections.Generic.IEnumerable<JObject> Objects(JToken token)
        {
            if (token is JObject obj) yield return obj;
            if (token is JContainer container)
            {
                foreach (var child in container.Children())
                {
                    foreach (var nested in Objects(child))
                        yield return nested;
                }
            }
        }

        static bool IsLight(string objectKind, JObject node)
            => Contains(objectKind, "light") ||
               HasAny(node, "LightIntensity", "lightIntensity", "Intensity", "intensity", "Range", "range");

        static void Attach(Scheme scheme, SObject obj)
        {
            if (obj == null) return;
            obj.Parent = scheme;
            scheme.Childrens.Add(obj);
            if (obj.GameObject != null && scheme.GameObject != null)
                obj.GameObject.transform.SetParent(scheme.GameObject.transform, true);
        }

        static bool TryReadPrimitiveType(JObject node, out PrimitiveType primitiveType)
        {
            var value = ReadString(node, "PrimitiveType", "primitiveType", "Primitive", "primitive", "Shape", "shape");
            if (int.TryParse(value, out var number) && System.Enum.IsDefined(typeof(PrimitiveType), number))
            {
                primitiveType = (PrimitiveType)number;
                return true;
            }

            if (System.Enum.TryParse(value, true, out primitiveType)) return true;
            primitiveType = PrimitiveType.Cube;
            return HasAny(node, "PrimitiveType", "primitiveType", "Primitive", "primitive", "Shape", "shape");
        }

        static Vector3 ReadVector(JObject node, Vector3 fallback, params string[] names)
        {
            var token = ReadToken(node, names);
            if (token == null) return fallback;
            if (token is JArray array)
                return new Vector3(
                    ReadArrayFloat(array, 0, fallback.x),
                    ReadArrayFloat(array, 1, fallback.y),
                    ReadArrayFloat(array, 2, fallback.z));
            if (token is JObject obj)
                return new Vector3(
                    ReadFloat(obj, fallback.x, "x", "X"),
                    ReadFloat(obj, fallback.y, "y", "Y"),
                    ReadFloat(obj, fallback.z, "z", "Z"));
            return fallback;
        }

        static Color ReadColor(JObject node, Color fallback)
        {
            var token = ReadToken(node, "Color", "color", "PrimitiveColor", "primitiveColor");
            if (token is JArray array)
                return new Color(
                    ReadArrayFloat(array, 0, fallback.r),
                    ReadArrayFloat(array, 1, fallback.g),
                    ReadArrayFloat(array, 2, fallback.b),
                    ReadArrayFloat(array, 3, fallback.a));
            if (token is JObject obj)
                return new Color(
                    ReadFloat(obj, fallback.r, "r", "R"),
                    ReadFloat(obj, fallback.g, "g", "G"),
                    ReadFloat(obj, fallback.b, "b", "B"),
                    ReadFloat(obj, fallback.a, "a", "A"));
            return fallback;
        }

        static JToken ReadToken(JObject node, params string[] names)
        {
            foreach (var name in names)
                if (node.TryGetValue(name, System.StringComparison.OrdinalIgnoreCase, out var token))
                    return token;
            return null;
        }

        static string ReadString(JObject node, params string[] names)
            => ReadToken(node, names)?.ToString() ?? string.Empty;

        static float ReadFloat(JObject node, float fallback, params string[] names)
        {
            var token = ReadToken(node, names);
            return token != null &&
                   float.TryParse(token.ToString(), System.Globalization.NumberStyles.Float,
                       System.Globalization.CultureInfo.InvariantCulture, out var value)
                ? value
                : fallback;
        }

        static float ReadArrayFloat(JArray array, int index, float fallback)
            => index < array.Count &&
               float.TryParse(array[index].ToString(), System.Globalization.NumberStyles.Float,
                   System.Globalization.CultureInfo.InvariantCulture, out var value)
                ? value
                : fallback;

        static bool ReadBool(JObject node, bool fallback, params string[] names)
        {
            var token = ReadToken(node, names);
            return token != null && bool.TryParse(token.ToString(), out var value) ? value : fallback;
        }

        static bool HasAny(JObject node, params string[] names)
            => names.Any(name => node.TryGetValue(name, System.StringComparison.OrdinalIgnoreCase, out _));

        static bool Contains(string value, string part)
            => value != null && value.IndexOf(part, System.StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
