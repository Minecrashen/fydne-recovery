using Qurre.API;
using System.Collections.Generic;
using Qurre.API.Controllers;

namespace Loli.HintsCore;

public class PlayerDisplay
{
    readonly Player pl;
    readonly HashSet<DisplayBlock> _blocks = new();

    public float CanvasWidth { get; private set; }

    internal PlayerDisplay(Player pl)
    {
        this.pl = pl;

        pl.Variables[Constants.VariableTag] = this;
    }


    public bool AddBlock(DisplayBlock block)
        => _blocks.Add(block);

    public bool ContainsBlock(DisplayBlock block)
        => _blocks.Contains(block);

    public bool RemoveBlock(DisplayBlock block)
    => _blocks.Remove(block);

    public IReadOnlyCollection<DisplayBlock> GetBlocks()
        => _blocks;

    public Player GetPlayer()
        => pl;


    internal void Prepare()
    {
        CanvasWidth = Constants.CanvasSafeHeight * pl.ReferenceHub.aspectRatioSync.AspectRatio;
    }
}