using Microsoft.EntityFrameworkCore;

namespace BuySales.WinForms.Data;

/// <summary>
/// SQLite 데이터베이스 컨텍스트를 생성합니다.
/// </summary>
public class SqliteBuySalesDbContextFactory : IBuySalesDbContextFactory
{
    /// <summary>
    /// SQLite 연결을 사용하는 새 데이터베이스 컨텍스트를 생성합니다.
    /// </summary>
    /// <returns>생성된 데이터베이스 컨텍스트입니다.</returns>
    public BuySalesDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BuySalesDbContext>()
            .UseSqlite($"Data Source={AppPaths.DatabasePath}")
            .Options;

        return new BuySalesDbContext(options);
    }
}
