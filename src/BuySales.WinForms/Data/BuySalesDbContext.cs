using BuySales.WinForms.Models;
using Microsoft.EntityFrameworkCore;

namespace BuySales.WinForms.Data;

/// <summary>
/// 매입과 매출 데이터를 관리하는 EF Core 데이터베이스 컨텍스트입니다.
/// </summary>
public class BuySalesDbContext : DbContext
{
    /// <summary>
    /// 지정된 옵션으로 데이터베이스 컨텍스트를 생성합니다.
    /// </summary>
    /// <param name="options">데이터베이스 연결 옵션입니다.</param>
    public BuySalesDbContext(DbContextOptions<BuySalesDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// 매입과 매출 거래 목록을 가져오거나 설정합니다.
    /// </summary>
    public DbSet<BuySaleTransaction> Transactions => Set<BuySaleTransaction>();

    /// <summary>
    /// 모델 매핑과 제약 조건을 구성합니다.
    /// </summary>
    /// <param name="modelBuilder">모델 생성기입니다.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BuySaleTransaction>(entity =>
        {
            entity.ToTable("BuySaleTransactions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ItemName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Memo).HasMaxLength(300);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.Quantity).HasPrecision(18, 2);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.HasIndex(x => x.TransactionDate);
            entity.HasIndex(x => x.Kind);
        });
    }
}
