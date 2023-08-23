using BBRAPIModules;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

public class RichText : BattleBitModule
{
    public string NewLine() => "<br>";

    public string Color(string? color = null)
    {
        if (color is null)
        {
            return Colors.White;
        }

        return $"<{color}>";
    }

    public string FromColorName(string colorName)
    {
        FieldInfo? color = typeof(Colors).GetFields().FirstOrDefault(x => x.Name.ToLower() == colorName.ToLower());
        if (color == null)
        {
            Console.WriteLine($"No color found with name {colorName}");
            return "<#FFFFFF>";
        }

        return color.GetValue(null)!.ToString();
    }

    public string Align(string? alignment = null)
    {
        if (alignment is null)
        {
            return Alignments.None;
        }

        return $"<align=\"{alignment}\">";
    }

    public string FromAlignmentName(string alignmentName)
    {
        FieldInfo? alignment = typeof(Alignments).GetFields().FirstOrDefault(x => x.Name.ToLower() == alignmentName.ToLower());
        if (alignment == null)
        {
            Console.WriteLine($"No alignment found with name {alignmentName}");
            return Alignments.None;
        }

        return alignment.GetValue(null)!.ToString();
    }

    public string Alpha(string? alpha = null)
    {
        if (alpha is null)
        {
            return "<alpha=#FF>";
        }

        return $"<alpha={alpha}>";
    }

    public string CharacterSpacing(int pixels) => $"<cspace={pixels}>";
    public string CharacterSpacing(float em) => $"<cspace={em}em>";

    public string Font(string? fontName)
    {
        if (fontName is null)
        {
            return "<font=\"default\">";
        }

        return $"<font=\"{fontName}\">";
    }

    public string Indent(int? percentage = null)
    {
        if (percentage is null)
        {
            return "</indent>";
        }

        return $"<indent={percentage}%>";
    }

    public string LineHeight(int percentage) => $"<line-height={percentage}%>";

    public string LineIndentation(int percentage) => $"<line-indent={percentage}%>";

    public string Lowercase(bool lowercase) => lowercase ? "<lowercase>" : "</lowercase>";
    public string Uppercase(bool uppercase) => uppercase ? "<uppercase>" : "</uppercase>";
    public string Smallcaps(bool smallcaps) => smallcaps ? "<smallcaps>" : "</smallcaps>";

    public string Margin(int pixels) => $"<margin={pixels}>";
    public string Margin(float em) => $"<margin={em}em>";

    public string Monospacing(int pixels) => $"<mspace={pixels}>";
    public string Monospacing(float em) => $"<mspace={em}em>";
    public string Monospacing() => "</mspace>";

    public string Noparse(bool noparse) => noparse ? "<noparse>" : "</noparse>";

    public string NonBreakingSpace(bool nonBreakingSpace) => nonBreakingSpace ? "<nobr>" : "</nobr>";

    public string HorizontalPosition(int percentage) => $"<pos={percentage}%>";

    public string HorizontalSpace(int pixels) => $"<space={pixels}>";
    public string HorizontalSpace(float em) => $"<space={em}em>";

    public string VerticalOffset(float em) => $"<voffset={em}em>";

    public string TextWidth(int percentage) => $"<width={percentage}%>";

    public string Sprite(string spriteName, string? color = null)
    {
        FieldInfo? sprite = typeof(Sprites).GetFields().FirstOrDefault(x => x.Name.ToLower() == spriteName.ToLower());
        if (sprite == null)
        {
            Console.WriteLine($"No sprite found with name {spriteName}");
            return string.Empty;
        }

        string spriteText = sprite.GetValue(null)!.ToString();
        if (!string.IsNullOrEmpty(color))
        {
            spriteText = spriteText.Replace("<sprite ", $"<sprite color={color} ");
        }

        return spriteText;
    }

    public string Bold(bool bold) => bold ? "<b>" : "</b>";

    public string Italic(bool italic) => italic ? "<i>" : "</i>";

    public string Mark(bool mark, string color = "#ffff00aa") => mark ? $"<mark color=\"{color}\">" : "</mark>";

    public string Strikethrough(bool strikethrough) => strikethrough ? "<s>" : "</s>";

    public string Underline(bool underline) => underline ? "<u>" : "</u>";

    public string Size(int percentage) => $"<size={percentage}%>";

    public string Subscript(bool subscript) => subscript ? "<sub>" : "</sub>";

    public string Superscript(bool superscript) => superscript ? "<sup>" : "</sup>";
}

public static class Alignments
{
    public static readonly string Left = "<align=\"left\">";
    public static readonly string Center = "<align=\"center\">";
    public static readonly string Right = "<align=\"right\">";
    public static readonly string None = "</align>";
}

public static class Colors
{
    public static readonly string Black = "<color=\"black\">";
    public static readonly string Blue = "<color=\"blue\">";
    public static readonly string Brown = "<color=\"brown\">";
    public static readonly string Cyan = "<color=\"cyan\">";
    public static readonly string Darkblue = "<color=\"darkblue\">";
    public static readonly string Green = "<color=\"green\">";
    public static readonly string Grey = "<color=\"grey\">";
    public static readonly string Lightblue = "<color=\"lightblue\">";
    public static readonly string Lime = "<color=\"lime\">";
    public static readonly string Magenta = "<color=\"magenta\">";
    public static readonly string Maroon = "<color=\"maroon\">";
    public static readonly string Navy = "<color=\"navy\">";
    public static readonly string Olive = "<color=\"olive\">";
    public static readonly string Orange = "<color=\"orange\">";
    public static readonly string Purple = "<color=\"purple\">";
    public static readonly string Red = "<color=\"red\">";
    public static readonly string Silver = "<color=\"silver\">";
    public static readonly string Teal = "<color=\"teal\">";
    public static readonly string White = "<color=\"white\">";
    public static readonly string Yellow = "<color=\"yellow\">";
    public static readonly string MediumVioletRed = "<#C71585>";
    public static readonly string DeepPink = "<#FF1493>";
    public static readonly string PaleVioletRed = "<#DB7093>";
    public static readonly string HotPink = "<#FF69B4>";
    public static readonly string LightPink = "<#FFB6C1>";
    public static readonly string Pink = "<#FFC0CB>";
    public static readonly string DarkRed = "<#8B0000>";
    public static readonly string Firebrick = "<#B22222>";
    public static readonly string Crimson = "<#DC143C>";
    public static readonly string IndianRed = "<#CD5C5C>";
    public static readonly string LightCoral = "<#F08080>";
    public static readonly string Salmon = "<#FA8072>";
    public static readonly string DarkSalmon = "<#E9967A>";
    public static readonly string LightSalmon = "<#FFA07A>";
    public static readonly string OrangeRed = "<#FF4500>";
    public static readonly string Tomato = "<#FF6347>";
    public static readonly string DarkOrange = "<#FF8C00>";
    public static readonly string Coral = "<#FF7F50>";
    public static readonly string DarkKhaki = "<#BDB76B>";
    public static readonly string Gold = "<#FFD700>";
    public static readonly string Khaki = "<#F0E68C>";
    public static readonly string PeachPuff = "<#FFDAB9>";
    public static readonly string PaleGoldenrod = "<#EEE8AA>";
    public static readonly string Moccasin = "<#FFE4B5>";
    public static readonly string PapayaWhip = "<#FFEFD5>";
    public static readonly string LightGoldenrodYellow = "<#FAFAD2>";
    public static readonly string LemonChiffon = "<#FFFACD>";
    public static readonly string LightYellow = "<#FFFFE0>";
    public static readonly string SaddleBrown = "<#8B4513>";
    public static readonly string Sienna = "<#A0522D>";
    public static readonly string Chocolate = "<#D2691E>";
    public static readonly string DarkGoldenrod = "<#B8860B>";
    public static readonly string Peru = "<#CD853F>";
    public static readonly string RosyBrown = "<#BC8F8F>";
    public static readonly string Goldenrod = "<#DAA520>";
    public static readonly string SandyBrown = "<#F4A460>";
    public static readonly string Tan = "<#D2B48C>";
    public static readonly string Burlywood = "<#DEB887>";
    public static readonly string Wheat = "<#F5DEB3>";
    public static readonly string NavajoWhite = "<#FFDEAD>";
    public static readonly string Bisque = "<#FFE4C4>";
    public static readonly string BlanchedAlmond = "<#FFEBCD>";
    public static readonly string Cornsilk = "<#FFF8DC>";
    public static readonly string Indigo = "<#4B0082>";
    public static readonly string DarkMagenta = "<#8B008B>";
    public static readonly string DarkViolet = "<#9400D3>";
    public static readonly string DarkSlateBlue = "<#483D8B>";
    public static readonly string BlueViolet = "<#8A2BE2>";
    public static readonly string DarkOrchid = "<#9932CC>";
    public static readonly string Fuchsia = "<#FF00FF>";
    public static readonly string SlateBlue = "<#6A5ACD>";
    public static readonly string MediumSlateBlue = "<#7B68EE>";
    public static readonly string MediumOrchid = "<#BA55D3>";
    public static readonly string MediumPurple = "<#9370DB>";
    public static readonly string Orchid = "<#DA70D6>";
    public static readonly string Violet = "<#EE82EE>";
    public static readonly string Plum = "<#DDA0DD>";
    public static readonly string Thistle = "<#D8BFD8>";
    public static readonly string Lavender = "<#E6E6FA>";
    public static readonly string MidnightBlue = "<#191970>";
    public static readonly string MediumBlue = "<#0000CD>";
    public static readonly string RoyalBlue = "<#4169E1>";
    public static readonly string SteelBlue = "<#4682B4>";
    public static readonly string DodgerBlue = "<#1E90FF>";
    public static readonly string DeepSkyBlue = "<#00BFFF>";
    public static readonly string CornflowerBlue = "<#6495ED>";
    public static readonly string SkyBlue = "<#87CEEB>";
    public static readonly string LightSkyBlue = "<#87CEFA>";
    public static readonly string LightSteelBlue = "<#B0C4DE>";
    public static readonly string PowderBlue = "<#B0E0E6>";
    public static readonly string DarkCyan = "<#008B8B>";
    public static readonly string LightSeaGreen = "<#20B2AA>";
    public static readonly string CadetBlue = "<#5F9EA0>";
    public static readonly string DarkTurquoise = "<#00CED1>";
    public static readonly string MediumTurquoise = "<#48D1CC>";
    public static readonly string Turquoise = "<#40E0D0>";
    public static readonly string Aqua = "<#00FFFF>";
    public static readonly string Aquamarine = "<#7FFFD4>";
    public static readonly string PaleTurquoise = "<#AFEEEE>";
    public static readonly string LightCyan = "<#E0FFFF>";
    public static readonly string DarkGreen = "<#006400>";
    public static readonly string DarkOliveGreen = "<#556B2F>";
    public static readonly string ForestGreen = "<#228B22>";
    public static readonly string SeaGreen = "<#2E8B57>";
    public static readonly string OliveDrab = "<#6B8E23>";
    public static readonly string MediumSeaGreen = "<#3CB371>";
    public static readonly string LimeGreen = "<#32CD32>";
    public static readonly string SpringGreen = "<#00FF7F>";
    public static readonly string MediumSpringGreen = "<#00FA9A>";
    public static readonly string DarkSeaGreen = "<#8FBC8F>";
    public static readonly string MediumAquamarine = "<#66CDAA>";
    public static readonly string YellowGreen = "<#9ACD32>";
    public static readonly string LawnGreen = "<#7CFC00>";
    public static readonly string Chartreuse = "<#7FFF00>";
    public static readonly string LightGreen = "<#90EE90>";
    public static readonly string GreenYellow = "<#ADFF2F>";
    public static readonly string PaleGreen = "<#98FB98>";
    public static readonly string MistyRose = "<#FFE4E1>";
    public static readonly string AntiqueWhite = "<#FAEBD7>";
    public static readonly string Linen = "<#FAF0E6>";
    public static readonly string Beige = "<#F5F5DC>";
    public static readonly string WhiteSmoke = "<#F5F5F5>";
    public static readonly string LavenderBlush = "<#FFF0F5>";
    public static readonly string OldLace = "<#FDF5E6>";
    public static readonly string AliceBlue = "<#F0F8FF>";
    public static readonly string Seashell = "<#FFF5EE>";
    public static readonly string GhostWhite = "<#F8F8FF>";
    public static readonly string Honeydew = "<#F0FFF0>";
    public static readonly string FloralWhite = "<#FFFAF0>";
    public static readonly string Azure = "<#F0FFFF>";
    public static readonly string MintCream = "<#F5FFFA>";
    public static readonly string Snow = "<#FFFAFA>";
    public static readonly string Ivory = "<#FFFFF0>";
    public static readonly string DarkSlateGray = "<#2F4F4F>";
    public static readonly string DimGray = "<#696969>";
    public static readonly string SlateGray = "<#708090>";
    public static readonly string Gray = "<#808080>";
    public static readonly string LightSlateGray = "<#778899>";
    public static readonly string DarkGray = "<#A9A9A9>";
    public static readonly string LightGray = "<#D3D3D3>";
    public static readonly string Gainsboro = "<#DCDCDC>";
}

public static class Sprites
{
    public static readonly string Moderator = "<sprite index=0>";
    public static readonly string Patreon = "<sprite index=1>";
    public static readonly string Creator = "<sprite index=2>";
    public static readonly string DiscordBooster = "<sprite index=3>";
    public static readonly string Special = "<sprite index=4>";
    public static readonly string PatreonFirebacker = "<sprite index=5>";
    public static readonly string Vip = "<sprite index=6>";
    public static readonly string Supporter = "<sprite index=7>";
    public static readonly string Developer = "<sprite index=8>";
    public static readonly string Veteran = "<sprite index=9>";
    public static readonly string Misc1 = "<sprite index=10>";
    public static readonly string Misc2 = "<sprite index=11>";
    public static readonly string Misc3 = "<sprite index=12>";
    public static readonly string Misc4 = "<sprite index=13>";
    public static readonly string Misc5 = "<sprite index=14>";
    public static readonly string Misc6 = "<sprite index=15>";
}