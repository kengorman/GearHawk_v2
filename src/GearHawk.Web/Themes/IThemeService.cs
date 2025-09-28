namespace GearHawk.Web.Themes;

public interface IThemeService
{
    ThemeId Current { get; }
    Task SetAsync (ThemeId themeId);
}
