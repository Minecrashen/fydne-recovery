using Qurre.API.Attributes;
using Qurre.Events;

namespace Loli.Controllers
{
    static class ConceptsController
    {
        static bool _activated = false;

        static internal bool IsActivated => _activated;

        static internal void Activate()
            => _activated = true;

        static internal void Disable()
            => _activated = false;


        [EventMethod(RoundEvents.Waiting)]
        static void Refresh()
        {
            _activated = false;
        }
    }
}