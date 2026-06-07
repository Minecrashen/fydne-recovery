using Qurre.API.Attributes;
using Qurre.Events;

namespace Loli.Concepts.Scp008
{
    static class RoomsData
    {
        static internal TubeRoom Lcz173 { get; set; }
        static internal TubeRoom Hcz049 { get; set; }
        static internal TubeRoom Hcz939 { get; set; }
        static internal TubeRoom EzVent { get; set; }

        static internal ControlRoom Control { get; set; }


        [EventMethod(RoundEvents.Waiting)]
        static void Init()
        {
            Lcz173 = new(TubeRoomType.Lcz173);
            Hcz049 = new(TubeRoomType.Hcz049);
            Hcz939 = new(TubeRoomType.Hcz939);
            EzVent = new(TubeRoomType.EzVent);

            Control = new();
        }
    }
}