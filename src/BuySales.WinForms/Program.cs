namespace BuySales.WinForms;

/// <summary>
/// 프로그램 시작 지점을 제공하는 클래스입니다.
/// </summary>
static class Program
{
    /// <summary>
    /// 애플리케이션을 초기화하고 메인 화면을 실행합니다.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
