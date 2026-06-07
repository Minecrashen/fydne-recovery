using Loli.HintsCore.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Loli.HintsCore;

public sealed class DisplayBlock
{
    public HashList<MessageBlock> Contents { get; }
    public Vector2 Position { get; set; }
    public Vector2 MaxSize { get; set; }
    public Color Background { get; set; }
    public Padding Padding { get; set; }
    public Align Align { get; }
    public bool NewFromTop { get; }

    public DisplayBlock(Vector2 position, Vector2 maxSize, Color background = default, Padding padding = default, Align align = Align.Center, bool newFromTop = true)
    {
        Contents = [];
        Position = position;
        MaxSize = maxSize;
        Background = background;
        Padding = padding;
        Align = align;
        NewFromTop = newFromTop;
    }


    public string GetText(PlayerDisplay display)
    {
        float realX;
        float realY = Position.y + Constants.CenterVOffset + Padding.Bottom;
        string reply = string.Empty;
        Vector2 size = Vector2.zero;

        float maxSizeY = MaxSize.y - Padding.Top - Padding.Bottom;
        float maxSizeX = MaxSize.x - Padding.Left - Padding.Right;

        if (Align == Align.Left)
        {
            if (Position.x < -display.CanvasWidth)
                realX = -Mathf.Abs((display.CanvasWidth - Constants.CanvasSafeWidth) / 2);
            else if (Position.x > display.CanvasWidth)
                realX = Mathf.Abs(display.CanvasWidth - ((display.CanvasWidth - Constants.CanvasSafeWidth) / 2)) - MaxSize.x;
            else
                realX = Position.x + (Constants.CanvasSafeWidth / 2);

            realX += Padding.Left;
        }
        else if (Align == Align.Right)
        {
            if (Position.x < -display.CanvasWidth)
                realX = -Mathf.Abs((display.CanvasWidth - Constants.CanvasSafeWidth) / 2) + MaxSize.x;
            else if (Position.x > display.CanvasWidth)
                realX = Mathf.Abs(display.CanvasWidth - ((display.CanvasWidth - Constants.CanvasSafeWidth) / 2));
            else
                realX = Position.x + (Constants.CanvasSafeWidth / 2);

            realX -= Padding.Right;
        }
        else
        {
            if (Position.x < -display.CanvasWidth + maxSizeX)
                realX = -display.CanvasWidth + (MaxSize.x / 2) + Padding.Left;
            else if (Position.x > display.CanvasWidth - maxSizeX)
                realX = display.CanvasWidth - (MaxSize.x / 2) - Padding.Right;
            else
                realX = Position.x;
        }

        //string markColor = Background.ToHex();

        for (int i = 0; i < Contents.Count; i++)
        {
            MessageBlock block = Contents[i];

            List<(string, float)> contentList = block.GetContents(display.GetPlayer(), this, maxSizeX, realX);
            string blockColor = block.Color.ToHex();

            if (!NewFromTop)
                contentList.Reverse();

            foreach ((string content, float sizeX) in contentList)
            {
                Vector2 blockSize = new(
                    x: sizeX,
                    y: Worker.CalculateContentSize($"<size={block.Size}>{content}</size>").y
                );

                if (size.y + blockSize.y > maxSizeY)
                    goto IL_001;

                bool sizeSetuped = !string.IsNullOrEmpty(block.Size);

                reply += $"<voffset={realY + (NewFromTop ? -size.y : size.y)}>" +
                    //$"<mark color={markColor}>" +
                    $"<color={blockColor}>" +
                    (sizeSetuped ? $"<size={block.Size}>" : "") +
                    $"{content}" +
                    (sizeSetuped ? "</size>" : "") +
                    $"</color>" +
                    //$"</mark>" +
                    $"\n";
                size = new(Mathf.Max(size.x, blockSize.x), size.y + blockSize.y);
            }

        }

        goto IL_001;

    IL_001:
        {
            /*
            float sizepx = size.y;
            string stepa = "a";

            float sizeXMax = size.x + (Padding * 2) + (size.x / 5);
            float sizeYMax = size.y + (Padding * 2);

            while (true)
            {
                Vector2 calc = Worker.CalculateContentSize($"<mark color=000000055><size={sizepx}>a</size></mark>");

                Log.Info($"CAAALC#1: {calc}");

                if (calc.y - 5 > sizeYMax)
                    break;

                if (calc.x > sizeXMax)
                    break;

                if (calc.x > 1000)
                    break;

                sizepx += 1;
            }

            while (true)
            {
                Vector2 calc = Worker.CalculateContentSize($"<mark color=000000055><size={sizepx}>{stepa}</size></mark>");

                Log.Info($"CAAALC#2: {calc}");

                if (calc.x > sizeXMax)
                    break;

                if (calc.x > 1000)
                    break;

                stepa += "a";
            }

            string mark = $"<mark color=000000055><size={sizepx}><color=#0000>{stepa}</color></size></mark>";
            Vector2 markSize = Worker.CalculateContentSize(mark);
            float maskUpY = (markSize.y / 2) - Padding;

            Log.Info("----------");
            Log.Info($"stepa: {stepa}");
            Log.Info($"sizepx: {sizepx}");
            Log.Info($"size: {size}");
            Log.Info($"calc reply: {Worker.CalculateContentSize(reply)}");
            Log.Info($"calc mark: {markSize}");
            Log.Info("----------");

            //<mark color=000000055><size=50><color=#0000>aaa</color></size></mark>
            return $"<voffset={realY + (NewFromTop ? -maskUpY : maskUpY)}><pos={realX}>" +
                mark +
                $"\n{reply}";
            */

            return reply;
        }

    }
}