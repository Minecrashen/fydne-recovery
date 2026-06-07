#if MRP
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Collections.Generic;

namespace Loli.Addons.RolePlay
{
    static class Safe079
    {
        static readonly List<Generator> ActGens = new();

        [EventMethod(RoundEvents.Waiting)]
        static void Waiting() => ActGens.Clear();

        [EventMethod(PlayerEvents.InteractGenerator)]
        static void GenAct(InteractGeneratorEvent ev)
        {
            if (ev.Status == GeneratorStatus.Activate)
                ActGens.Add(ev.Generator);
            else if (ev.Status == GeneratorStatus.Deactivate && ActGens.Contains(ev.Generator))
                ActGens.Remove(ev.Generator);
        }

        [EventMethod(ScpEvents.ActivateGenerator)]
        static void GenAct(ActivateGeneratorEvent ev)
        {
            if (!ActGens.Contains(ev.Generator))
                ev.Allowed = false;
        }
    }
}
#endif