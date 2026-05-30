namespace BuySales.WinForms.Theme;

/// <summary>
/// 테마별 화면 색상 팔레트를 제공합니다.
/// </summary>
public class ThemePalette
{
    /// <summary>
    /// 화면 배경색을 가져오거나 설정합니다.
    /// </summary>
    public Color Background { get; set; }

    /// <summary>
    /// 패널 배경색을 가져오거나 설정합니다.
    /// </summary>
    public Color Panel { get; set; }

    /// <summary>
    /// 카드 배경색을 가져오거나 설정합니다.
    /// </summary>
    public Color Card { get; set; }

    /// <summary>
    /// 기본 글자색을 가져오거나 설정합니다.
    /// </summary>
    public Color Foreground { get; set; }

    /// <summary>
    /// 흐린 글자색을 가져오거나 설정합니다.
    /// </summary>
    public Color Muted { get; set; }

    /// <summary>
    /// 강조색을 가져오거나 설정합니다.
    /// </summary>
    public Color Accent { get; set; }

    /// <summary>
    /// 경계선 색을 가져오거나 설정합니다.
    /// </summary>
    public Color Border { get; set; }

    /// <summary>
    /// 현재 테마에 맞는 색상 팔레트를 생성합니다.
    /// </summary>
    /// <param name="theme">화면 테마입니다.</param>
    /// <returns>색상 팔레트입니다.</returns>
    public static ThemePalette FromTheme(AppTheme theme)
    {
        if (theme == AppTheme.Light)
        {
            return new ThemePalette
            {
                Background = Color.FromArgb(241, 245, 249),
                Panel = Color.FromArgb(226, 232, 240),
                Card = Color.White,
                Foreground = Color.FromArgb(15, 23, 42),
                Muted = Color.FromArgb(71, 85, 105),
                Accent = Color.FromArgb(13, 110, 253),
                Border = Color.FromArgb(203, 213, 225)
            };
        }

        return new ThemePalette
        {
            Background = Color.FromArgb(17, 24, 39),
            Panel = Color.FromArgb(31, 41, 55),
            Card = Color.FromArgb(43, 52, 69),
            Foreground = Color.FromArgb(248, 250, 252),
            Muted = Color.FromArgb(203, 213, 225),
            Accent = Color.FromArgb(56, 189, 248),
            Border = Color.FromArgb(75, 85, 99)
        };
    }
}
