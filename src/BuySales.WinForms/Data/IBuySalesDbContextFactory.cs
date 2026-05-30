namespace BuySales.WinForms.Data;

/// <summary>
/// 데이터베이스 공급자별 컨텍스트 생성을 추상화합니다.
/// </summary>
public interface IBuySalesDbContextFactory
{
    /// <summary>
    /// 새 데이터베이스 컨텍스트를 생성합니다.
    /// </summary>
    /// <returns>생성된 데이터베이스 컨텍스트입니다.</returns>
    BuySalesDbContext CreateDbContext();
}
