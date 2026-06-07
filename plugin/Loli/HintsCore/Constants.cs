using System.Text.RegularExpressions;

namespace Loli.HintsCore;

public static class Constants
{
    public const string VariableTag = "PlayerDisplay";

    public const float CanvasSafeWidth = 1200;
    public const float CanvasSafeHeight = 1080;
    public const float HintDisplayAbsVOffset = 145.0f;
    public const float MaxVOffset = CanvasSafeHeight + (2 * HintDisplayAbsVOffset) + 30;
    public const float CenterVOffset = (MaxVOffset / 2) + HintDisplayAbsVOffset;

    public static string[] SmallChars = new string[] { "|", "i", ":" };
    public static readonly Regex ReplaceRegex = new("((\\<(voffset|pos|align|line\\-height)\\=)+([a-zA-Z0-9_%-.])+(\\>))|(\\</(voffset|pos|align)\\>)");
    public static readonly Regex ReplaceSizeRegex = new("((\\<(size)\\=)+([a-zA-Z0-9_%-.])+(\\>))|(\\</(size)\\>)");
    public static readonly Regex ReplaceAllRegex = new("((\\<)+([a-zA-Z_-])+(\\=)+([a-zA-Z0-9_%-.])+(\\>))|(\\</)+([a-zA-Z_-])+(\\>)");
#if MRP
    public static readonly Regex ReplaceUnAllowedRegex = new("(\\<)|(\\>)|(\\[)|(\\])|(\\\\u003c)|(\\\\u003e)");
#elif NR
    public static readonly Regex ReplaceUnAllowedRegex = new("(\\<)|(\\>)|(\\[)|(\\])|(\\\\u003c)|(\\\\u003e)", RegexOptions.IgnoreCase);
#endif
}