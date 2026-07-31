using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ArenaDefender.Desktop.Rendering;

/// <summary>
/// A tiny hand-made font. Each character is a 5x7 grid of on/off dots drawn as
/// little squares, so the game can show text without loading any font files.
/// </summary>
public sealed class PixelFont
{
    private const int GlyphWidth = 5;
    private const int GlyphHeight = 7;
    private const int GlyphSpacing = 1;

    private readonly Primitives _primitives;
    private static readonly Dictionary<char, bool[,]> Glyphs = BuildGlyphs();

    /// <summary>Creates the font, drawing its dots with the given primitives helper.</summary>
    public PixelFont(Primitives primitives)
    {
        _primitives = primitives ?? throw new ArgumentNullException(nameof(primitives));
    }

    /// <summary>Returns the pixel size of a piece of text at the given scale.</summary>
    public Vector2 Measure(string text, int scale)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Vector2.Zero;
        }

        int width = text.Length * (GlyphWidth + GlyphSpacing) - GlyphSpacing;
        return new Vector2(width * scale, GlyphHeight * scale);
    }

    /// <summary>Draws text at a position, one dot-square at a time.</summary>
    public void Draw(SpriteBatch batch, string text, Vector2 position, Color color, int scale)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        float cursorX = position.X;
        foreach (char raw in text)
        {
            char c = char.ToUpperInvariant(raw);
            if (Glyphs.TryGetValue(c, out bool[,]? glyph))
            {
                for (int row = 0; row < GlyphHeight; row++)
                {
                    for (int col = 0; col < GlyphWidth; col++)
                    {
                        if (glyph[row, col])
                        {
                            _primitives.FillRectangle(
                                batch,
                                cursorX + col * scale,
                                position.Y + row * scale,
                                scale,
                                scale,
                                color);
                        }
                    }
                }
            }

            cursorX += (GlyphWidth + GlyphSpacing) * scale;
        }
    }

    /// <summary>Draws text centred horizontally on the given x position.</summary>
    public void DrawCentered(SpriteBatch batch, string text, float centerX, float y, Color color, int scale)
    {
        Vector2 size = Measure(text, scale);
        Draw(batch, text, new Vector2(centerX - size.X / 2f, y), color, scale);
    }

    private static bool[,] ToGlyph(string[] rows)
    {
        var glyph = new bool[GlyphHeight, GlyphWidth];
        for (int r = 0; r < GlyphHeight; r++)
        {
            for (int c = 0; c < GlyphWidth; c++)
            {
                glyph[r, c] = c < rows[r].Length && rows[r][c] == 'X';
            }
        }

        return glyph;
    }

    private static Dictionary<char, bool[,]> BuildGlyphs()
    {
        var g = new Dictionary<char, bool[,]>
        {
            [' '] = ToGlyph(new[] { "     ", "     ", "     ", "     ", "     ", "     ", "     " }),
            ['A'] = ToGlyph(new[] { " XXX ", "X   X", "X   X", "XXXXX", "X   X", "X   X", "X   X" }),
            ['B'] = ToGlyph(new[] { "XXXX ", "X   X", "X   X", "XXXX ", "X   X", "X   X", "XXXX " }),
            ['C'] = ToGlyph(new[] { " XXXX", "X    ", "X    ", "X    ", "X    ", "X    ", " XXXX" }),
            ['D'] = ToGlyph(new[] { "XXXX ", "X   X", "X   X", "X   X", "X   X", "X   X", "XXXX " }),
            ['E'] = ToGlyph(new[] { "XXXXX", "X    ", "X    ", "XXXX ", "X    ", "X    ", "XXXXX" }),
            ['F'] = ToGlyph(new[] { "XXXXX", "X    ", "X    ", "XXXX ", "X    ", "X    ", "X    " }),
            ['G'] = ToGlyph(new[] { " XXXX", "X    ", "X    ", "X  XX", "X   X", "X   X", " XXXX" }),
            ['H'] = ToGlyph(new[] { "X   X", "X   X", "X   X", "XXXXX", "X   X", "X   X", "X   X" }),
            ['I'] = ToGlyph(new[] { "XXXXX", "  X  ", "  X  ", "  X  ", "  X  ", "  X  ", "XXXXX" }),
            ['J'] = ToGlyph(new[] { "XXXXX", "   X ", "   X ", "   X ", "   X ", "X  X ", " XX  " }),
            ['K'] = ToGlyph(new[] { "X   X", "X  X ", "X X  ", "XX   ", "X X  ", "X  X ", "X   X" }),
            ['L'] = ToGlyph(new[] { "X    ", "X    ", "X    ", "X    ", "X    ", "X    ", "XXXXX" }),
            ['M'] = ToGlyph(new[] { "X   X", "XX XX", "X X X", "X X X", "X   X", "X   X", "X   X" }),
            ['N'] = ToGlyph(new[] { "X   X", "XX  X", "XX  X", "X X X", "X  XX", "X  XX", "X   X" }),
            ['O'] = ToGlyph(new[] { " XXX ", "X   X", "X   X", "X   X", "X   X", "X   X", " XXX " }),
            ['P'] = ToGlyph(new[] { "XXXX ", "X   X", "X   X", "XXXX ", "X    ", "X    ", "X    " }),
            ['Q'] = ToGlyph(new[] { " XXX ", "X   X", "X   X", "X   X", "X X X", "X  X ", " XX X" }),
            ['R'] = ToGlyph(new[] { "XXXX ", "X   X", "X   X", "XXXX ", "X X  ", "X  X ", "X   X" }),
            ['S'] = ToGlyph(new[] { " XXXX", "X    ", "X    ", " XXX ", "    X", "    X", "XXXX " }),
            ['T'] = ToGlyph(new[] { "XXXXX", "  X  ", "  X  ", "  X  ", "  X  ", "  X  ", "  X  " }),
            ['U'] = ToGlyph(new[] { "X   X", "X   X", "X   X", "X   X", "X   X", "X   X", " XXX " }),
            ['V'] = ToGlyph(new[] { "X   X", "X   X", "X   X", "X   X", "X   X", " X X ", "  X  " }),
            ['W'] = ToGlyph(new[] { "X   X", "X   X", "X   X", "X X X", "X X X", "XX XX", "X   X" }),
            ['X'] = ToGlyph(new[] { "X   X", "X   X", " X X ", "  X  ", " X X ", "X   X", "X   X" }),
            ['Y'] = ToGlyph(new[] { "X   X", "X   X", " X X ", "  X  ", "  X  ", "  X  ", "  X  " }),
            ['Z'] = ToGlyph(new[] { "XXXXX", "    X", "   X ", "  X  ", " X   ", "X    ", "XXXXX" }),
            ['0'] = ToGlyph(new[] { " XXX ", "X   X", "X  XX", "X X X", "XX  X", "X   X", " XXX " }),
            ['1'] = ToGlyph(new[] { "  X  ", " XX  ", "  X  ", "  X  ", "  X  ", "  X  ", " XXX " }),
            ['2'] = ToGlyph(new[] { " XXX ", "X   X", "    X", "   X ", "  X  ", " X   ", "XXXXX" }),
            ['3'] = ToGlyph(new[] { "XXXXX", "   X ", "  X  ", "   X ", "    X", "X   X", " XXX " }),
            ['4'] = ToGlyph(new[] { "   X ", "  XX ", " X X ", "X  X ", "XXXXX", "   X ", "   X " }),
            ['5'] = ToGlyph(new[] { "XXXXX", "X    ", "XXXX ", "    X", "    X", "X   X", " XXX " }),
            ['6'] = ToGlyph(new[] { " XXX ", "X    ", "X    ", "XXXX ", "X   X", "X   X", " XXX " }),
            ['7'] = ToGlyph(new[] { "XXXXX", "    X", "   X ", "  X  ", " X   ", " X   ", " X   " }),
            ['8'] = ToGlyph(new[] { " XXX ", "X   X", "X   X", " XXX ", "X   X", "X   X", " XXX " }),
            ['9'] = ToGlyph(new[] { " XXX ", "X   X", "X   X", " XXXX", "    X", "    X", " XXX " }),
            [':'] = ToGlyph(new[] { "     ", "  X  ", "  X  ", "     ", "  X  ", "  X  ", "     " }),
            ['.'] = ToGlyph(new[] { "     ", "     ", "     ", "     ", "     ", "  X  ", "  X  " }),
            [','] = ToGlyph(new[] { "     ", "     ", "     ", "     ", "     ", "  X  ", " X   " }),
            ['!'] = ToGlyph(new[] { "  X  ", "  X  ", "  X  ", "  X  ", "  X  ", "     ", "  X  " }),
            ['?'] = ToGlyph(new[] { " XXX ", "X   X", "    X", "   X ", "  X  ", "     ", "  X  " }),
            ['-'] = ToGlyph(new[] { "     ", "     ", "     ", "XXXXX", "     ", "     ", "     " }),
            ['+'] = ToGlyph(new[] { "     ", "  X  ", "  X  ", "XXXXX", "  X  ", "  X  ", "     " }),
            ['/'] = ToGlyph(new[] { "    X", "    X", "   X ", "  X  ", " X   ", "X    ", "X    " }),
            ['%'] = ToGlyph(new[] { "XX  X", "XX X ", "   X ", "  X  ", " X XX", "X  XX", "     " }),
            ['>'] = ToGlyph(new[] { "X    ", " X   ", "  X  ", "   X ", "  X  ", " X   ", "X    " }),
            ['<'] = ToGlyph(new[] { "    X", "   X ", "  X  ", " X   ", "  X  ", "   X ", "    X" }),
            ['('] = ToGlyph(new[] { "   X ", "  X  ", " X   ", " X   ", " X   ", "  X  ", "   X " }),
            [')'] = ToGlyph(new[] { " X   ", "  X  ", "   X ", "   X ", "   X ", "  X  ", " X   " }),
            ['\''] = ToGlyph(new[] { "  X  ", "  X  ", "  X  ", "     ", "     ", "     ", "     " }),
        };

        return g;
    }
}
