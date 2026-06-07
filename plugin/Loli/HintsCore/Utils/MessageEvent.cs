using Qurre.API;
using Qurre.API.Controllers;

namespace Loli.HintsCore.Utils;

public readonly struct MessageEvent
{
    public MessageBlock MessageBlock { get; }
    public Player Player { get; }

    public MessageEvent(MessageBlock messageBlock, Player player)
    {
        MessageBlock = messageBlock;
        Player = player;
    }
}