// Точка входа LabAPI: LabApi сам грузит плагины (классы-наследники Plugin).
// Этот класс LabApi обнаружит и вызовет Enable() → бутстрап Qurre-плагинов (Loli).
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Qurre
{
    public sealed class QurreBootstrap : LabApi.Loader.Features.Plugins.Plugin
    {
        public override string Name => "Qurre-Shim";
        public override string Description => "Qurre→LabAPI compatibility shim (FYDNE recovery)";
        public override string Author => "fydne-recovery";
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredApiVersion => new Version(1, 1, 0);

        public override void Enable()
        {
            LoadSiblingAssemblies();
            Qurre.API.Core.BootstrapAll();
        }
        public override void Disable() => Qurre.API.Core.ShutdownAll();

        static void LoadSiblingAssemblies()
        {
            string ownPath = Assembly.GetExecutingAssembly().Location;
            string dir = Path.GetDirectoryName(ownPath);
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;

            var loaded = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .Select(a =>
                {
                    try { return Path.GetFullPath(a.Location); }
                    catch { return string.Empty; }
                })
                .Where(path => !string.IsNullOrEmpty(path))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (string dll in Directory.GetFiles(dir, "*.dll"))
            {
                string full = Path.GetFullPath(dll);
                if (loaded.Contains(full)) continue;

                try { Assembly.LoadFrom(full); }
                catch (Exception ex) { Qurre.API.Log.Error($"Qurre-shim: failed to load sibling assembly {Path.GetFileName(dll)}: {ex.Message}"); }
            }
        }
    }
}
