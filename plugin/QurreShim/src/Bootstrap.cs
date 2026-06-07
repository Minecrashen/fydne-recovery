// Точка входа LabAPI: LabApi сам грузит плагины (классы-наследники Plugin).
// Этот класс LabApi обнаружит и вызовет Enable() → бутстрап Qurre-плагинов (Loli).
using System;

namespace Qurre
{
    public sealed class QurreBootstrap : LabApi.Loader.Features.Plugins.Plugin
    {
        public override string Name => "Qurre-Shim";
        public override string Description => "Qurre→LabAPI compatibility shim (FYDNE recovery)";
        public override string Author => "fydne-recovery";
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredApiVersion => new Version(1, 1, 0);

        public override void Enable() => Qurre.API.Core.BootstrapAll();
        public override void Disable() => Qurre.API.Core.ShutdownAll();
    }
}
