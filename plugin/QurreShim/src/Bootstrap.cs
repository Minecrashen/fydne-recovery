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

        // Загружаем Qurre-плагины (Loli.dll и т.п.), лежащие В ТОЙ ЖЕ папке, что и Qurre.dll.
        // Раньше грузились ВСЕ dll из ВСЕХ подпапок LabAPI/plugins с дедупом по пути файла — это
        // (а) тянуло чужие зависимости, которые LabAPI-лоадер сознательно не грузил, и (б) могло
        // загрузить одну сборку дважды из разных путей → расщепление идентичности типов
        // (InvalidCastException). Теперь: только своя папка + дедуп по имени сборки (AssemblyName.Name).
        static void LoadSiblingAssemblies()
        {
            var loadedNames = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (a.IsDynamic) continue;
                try { loadedNames.Add(a.GetName().Name); } catch { }
            }

            string dir = ShimDirectory();
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;

            foreach (string dll in Directory.GetFiles(dir, "*.dll"))
            {
                string name;
                try { name = AssemblyName.GetAssemblyName(dll).Name; }
                catch { continue; } // не управляемая .NET-сборка — пропускаем

                if (!loadedNames.Add(name)) continue; // уже загружена по имени

                try { Assembly.LoadFrom(Path.GetFullPath(dll)); }
                catch (Exception ex)
                {
                    loadedNames.Remove(name);
                    Qurre.API.Log.Error($"Qurre-shim: failed to load sibling assembly {Path.GetFileName(dll)}: {ex.Message}");
                }
            }
        }

        static string ShimDirectory()
        {
            try
            {
                string loc = typeof(QurreBootstrap).Assembly.Location;
                if (!string.IsNullOrWhiteSpace(loc))
                    return Path.GetDirectoryName(Path.GetFullPath(loc));
            }
            catch { }

            try
            {
                string codeBase = typeof(QurreBootstrap).Assembly.CodeBase;
                if (!string.IsNullOrWhiteSpace(codeBase))
                    return Path.GetDirectoryName(new Uri(codeBase).LocalPath);
            }
            catch { }

            return null;
        }
    }
}
