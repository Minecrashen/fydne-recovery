namespace Loli.HintsCore.Utils;

public struct Padding
{
    public float Top { get; set; }
    public float Right { get; set; }
    public float Bottom { get; set; }
    public float Left { get; set; }

    public Padding(float top, float right, float bottom, float left)
    {
        Top = top;
        Right = right;
        Bottom = bottom;
        Left = left;
    }

    public Padding(float verticale, float horiz)
    {
        Top = verticale;
        Right = horiz;
        Bottom = verticale;
        Left = horiz;
    }

    public Padding(float value)
    {
        Top = value;
        Right = value;
        Bottom = value;
        Left = value;
    }

    public Padding()
    {
        Top = 0;
        Right = 0;
        Bottom = 0;
        Left = 0;
    }
}