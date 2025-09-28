namespace GearHawk.Web.Themes;

public sealed record ThemePalette
{
    public string Background { get; init; }
    public string BackgroundElevated { get; init; }
    public string Text { get; init; }
    public string TextMuted { get; init; }
    public string Accent { get; init; }
    public string Accent2 { get; init; }
    public string Border { get; init; }
    public string? FontFamilyPrimary { get; init; }
    public string? FontFamilySecondary { get; init; }

    public ThemePalette(
        string background,
        string backgroundElevated,
        string text,
        string textMuted,
        string accent,
        string accent2,
        string border,
        string? fontFamilyPrimary,
        string? fontFamilySecondary)
    {
        Background = NotEmpty(background, nameof(background));
        BackgroundElevated = NotEmpty(backgroundElevated, nameof(backgroundElevated));
        Text = NotEmpty(text, nameof(text));
        TextMuted = NotEmpty(textMuted, nameof(textMuted));
        Accent = NotEmpty(accent, nameof(accent));
        Accent2 = NotEmpty(accent2, nameof(accent2));
        Border = NotEmpty(border, nameof(border));
        FontFamilyPrimary = fontFamilyPrimary;
        FontFamilySecondary = fontFamilySecondary;
    }

    private static string NotEmpty(string value, string name) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{name} cannot be null or whitespace.", name)
            : value;
}
