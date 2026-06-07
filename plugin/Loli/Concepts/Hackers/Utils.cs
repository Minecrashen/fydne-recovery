using UnityEngine;

namespace Loli.Concepts.Hackers;

static class Utils
{
    static internal Color GetRandomMonitorColor()
    {
        return Random.Range(0, 5) switch
        {
            0 => Color.cyan,
            1 => Color.green,
            2 => Color.blue,
            3 => Color.magenta,
            4 => Color.gray,
            _ => Color.red,
        };
    }

    static internal Color GetRoomColor(HackMode mode)
    {
        return mode switch
        {
            HackMode.Hacked => Color.red,
            HackMode.Hacking => Color.yellow,
            _ => Color.white,
        };
    }
}
