namespace BuySales.WinForms.Data;

/// <summary>
/// 애플리케이션 데이터베이스 초기화를 담당합니다.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// 데이터베이스와 기본 테이블이 없으면 생성합니다.
    /// </summary>
    /// <param name="contextFactory">데이터베이스 컨텍스트 팩터리입니다.</param>
    public static void EnsureCreated(IBuySalesDbContextFactory contextFactory)
    {
        using var context = contextFactory.CreateDbContext();
        context.Database.EnsureCreated();
    }
}
