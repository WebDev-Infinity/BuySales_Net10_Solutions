namespace BuySales.WinForms.Data;

/// <summary>
/// 애플리케이션 데이터 파일 경로를 제공합니다.
/// </summary>
public static class AppPaths
{
    /// <summary>
    /// 사용자 데이터 폴더 경로를 가져옵니다.
    /// </summary>
    public static string DataDirectory
    {
        get
        {
            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BuySalesNet10");

            Directory.CreateDirectory(directory);
            return directory;
        }
    }

    /// <summary>
    /// SQLite 데이터베이스 파일 경로를 가져옵니다.
    /// </summary>
    public static string DatabasePath => Path.Combine(DataDirectory, "buysales.db");

    /// <summary>
    /// 사용자 설정 파일 경로를 가져옵니다.
    /// </summary>
    public static string SettingsPath => Path.Combine(DataDirectory, "settings.txt");
}
