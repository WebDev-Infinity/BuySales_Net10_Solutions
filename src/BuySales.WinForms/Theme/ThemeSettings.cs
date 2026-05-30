using BuySales.WinForms.Data;

namespace BuySales.WinForms.Theme;

/// <summary>
/// 사용자 테마 설정을 파일에 저장하고 불러옵니다.
/// </summary>
public static class ThemeSettings
{
    /// <summary>
    /// 저장된 테마를 불러오며 기본값은 다크 테마입니다.
    /// </summary>
    /// <returns>현재 테마입니다.</returns>
    public static AppTheme Load()
    {
        if (!File.Exists(AppPaths.SettingsPath))
        {
            return AppTheme.Dark;
        }

        var value = File.ReadAllText(AppPaths.SettingsPath).Trim();
        return Enum.TryParse<AppTheme>(value, true, out var theme)
            ? theme
            : AppTheme.Dark;
    }

    /// <summary>
    /// 사용자가 선택한 테마를 저장합니다.
    /// </summary>
    /// <param name="theme">저장할 테마입니다.</param>
    public static void Save(AppTheme theme)
    {
        File.WriteAllText(AppPaths.SettingsPath, theme.ToString());
    }
}
