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
            var loaded = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .Select(a =>
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(a.Location)) return string.Empty;
                        return Path.GetFullPath(a.Location);
                    }
                    catch { return string.Empty; }
                })
                .Where(path => !string.IsNullOrEmpty(path))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (string dir in GetPluginDirectories())
            {
                if (!Directory.Exists(dir)) continue;

                foreach (string dll in Directory.GetFiles(dir, "*.dll"))
                {
                    string full = Path.GetFullPath(dll);
                    if (loaded.Contains(full)) continue;

                    try
                    {
                        Assembly.LoadFrom(full);
                        loaded.Add(full);
                    }
                    catch (Exception ex)
                    {
                        Qurre.API.Log.Error($"Qurre-shim: failed to load sibling assembly {Path.GetFileName(dll)}: {ex.Message}");
                    }
                }
            }
        }

        static string[] GetPluginDirectories()
        {
            var dirs = new System.Collections.Generic.List<string>();
            AddAssemblyDirectory(dirs, Assembly.GetExecutingAssembly());
            AddAssemblyDirectory(dirs, typeof(QurreBootstrap).Assembly);

            string root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!string.IsNullOrWhiteSpace(root))
            {
                string plugins = Path.Combine(root, "SCP Secret Laboratory", "LabAPI", "plugins");
                if (Directory.Exists(plugins))
                {
                    foreach (string dir in Directory.GetDirectories(plugins))
                        dirs.Add(dir);
                }
            }

            return dirs
                .Where(dir => !string.IsNullOrWhiteSpace(dir))
                .Select(dir =>
                {
                    try { return Path.GetFullPath(dir); }
                    catch { return string.Empty; }
                })
                .Where(dir => !string.IsNullOrEmpty(dir))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        static void AddAssemblyDirectory(System.Collections.Generic.List<string> dirs, Assembly assembly)
        {
            AddPathDirectory(dirs, assembly.Location);

            try
            {
                string codeBase = assembly.CodeBase;
                if (!string.IsNullOrWhiteSpace(codeBase))
                    AddPathDirectory(dirs, new Uri(codeBase).LocalPath);
            }
            catch { }
        }

        static void AddPathDirectory(System.Collections.Generic.List<string> dirs, string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;

            try
            {
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(dir))
                    dirs.Add(dir);
            }
            catch { }
        }
    }
}
