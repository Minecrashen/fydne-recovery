using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Collections.Generic;
using System.IO;

namespace Loli.Modules.Voices
{
    static class Generators
    {
        static string DirectoryPath { get; } = Path.Combine(Path.Combine(Paths.Plugins, "Audio"), "Generators");
        static internal string First { get; } = Path.Combine(DirectoryPath, "first.raw");
        static internal string Second { get; } = Path.Combine(DirectoryPath, "second.raw");
        static internal string Third { get; } = Path.Combine(DirectoryPath, "third.raw");
        static internal string Unknow { get; } = Path.Combine(DirectoryPath, "unknow.raw"); // unknow generator
        static internal string Enrage { get; } = Path.Combine(DirectoryPath, "enrage.raw"); // all generators activated
        static internal string Reload { get; } = Path.Combine(DirectoryPath, "reload.raw"); // overcharge
        static internal string OpenPlay { get; } = Path.Combine(DirectoryPath, "_open.raw");
        static internal string ClosePlay { get; } = Path.Combine(DirectoryPath, "_close.raw");


        [EventMethod(ScpEvents.Scp079Recontain)]
        static void Recontain()
        {
#if MRP
            VoiceCore.PlayInIntercom(Reload, "C.A.S.S.I.E.");
#elif NR
            AudioExtensions.PlayInIntercom(Reload, "C.A.S.S.I.E.");
#endif
        }

        [EventMethod(ScpEvents.GeneratorStatus)]
        static void Recontain(GeneratorStatusEvent ev)
        {
            if (ev.EnragedCount < 1)
                return;

            List<string> pathes = new()
            {
                OpenPlay
            };

            if (ev.EnragedCount == 1)
            {
                pathes.Add(First);
            }
            else if (ev.EnragedCount == 2)
            {
                pathes.Add(Second);
            }
            else if (ev.EnragedCount == 3)
            {
                pathes.Add(Third);
            }
            else
            {
                pathes.Add(Unknow);
            }

            if (ev.EnragedCount >= ev.TotalCount)
            {
                pathes.Add(Enrage);
            }
            else
            {
                pathes.Add(ClosePlay);
            }

            VoiceCore.PlayAudio(pathes);

        }
    }
}