using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;

namespace Loli.Controllers
{
    static class AlphaController
    {
        static bool _locked = false;
        static bool _lockedChange = false;

        static internal bool IsLocked => _locked;

        static internal void ChangeState(bool @new, bool always = false)
        {
            if (!@new && _lockedChange)
                return;

            _locked = @new;

            if (always)
                _lockedChange = true;
        }

        static internal void DisableLock()
            => _lockedChange = false;


        [EventMethod(AlphaEvents.Start, int.MinValue)]
        static void AntiDisable(AlphaStartEvent ev)
        {
            if (!IsLocked)
                return;

            ev.Allowed = false;
        }

        [EventMethod(RoundEvents.Waiting)]
        static void Refresh()
        {
            _locked = false;
            _lockedChange = false;
        }
    }
}