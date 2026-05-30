using BuySales.WinForms.Data;
using BuySales.WinForms.Models;
using Microsoft.EntityFrameworkCore;

namespace BuySales.WinForms.Services;

/// <summary>
/// 매입과 매출 거래 저장, 조회, 삭제 기능을 제공합니다.
/// </summary>
public class TransactionService
{
    private readonly IBuySalesDbContextFactory _contextFactory;

    /// <summary>
    /// 거래 서비스 인스턴스를 생성합니다.
    /// </summary>
    /// <param name="contextFactory">데이터베이스 컨텍스트 팩터리입니다.</param>
    public TransactionService(IBuySalesDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// 지정한 월의 거래 목록을 조회합니다.
    /// </summary>
    /// <param name="month">조회할 월에 포함된 날짜입니다.</param>
    /// <returns>거래 목록입니다.</returns>
    public async Task<List<BuySaleTransaction>> GetMonthlyTransactionsAsync(DateOnly month)
    {
        var start = new DateOnly(month.Year, month.Month, 1);
        var end = start.AddMonths(1);

        using var context = _contextFactory.CreateDbContext();
        return await context.Transactions
            .Where(x => x.TransactionDate >= start && x.TransactionDate < end)
            .OrderByDescending(x => x.TransactionDate)
            .ThenByDescending(x => x.Id)
            .ToListAsync();
    }

    /// <summary>
    /// 거래를 새로 저장하거나 기존 거래를 수정합니다.
    /// </summary>
    /// <param name="transaction">저장할 거래입니다.</param>
    /// <returns>비동기 작업입니다.</returns>
    public async Task SaveAsync(BuySaleTransaction transaction)
    {
        using var context = _contextFactory.CreateDbContext();

        if (transaction.Id == 0)
        {
            transaction.CreatedAt = DateTime.Now;
            context.Transactions.Add(transaction);
        }
        else
        {
            transaction.UpdatedAt = DateTime.Now;
            context.Transactions.Update(transaction);
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// 지정한 거래를 삭제합니다.
    /// </summary>
    /// <param name="id">삭제할 거래 고유 번호입니다.</param>
    /// <returns>비동기 작업입니다.</returns>
    public async Task DeleteAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var transaction = await context.Transactions.FindAsync(id);

        if (transaction is null)
        {
            return;
        }

        context.Transactions.Remove(transaction);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// 지정한 기간의 매입과 매출 합계를 조회합니다.
    /// </summary>
    /// <param name="start">조회 시작일입니다.</param>
    /// <param name="end">조회 종료일입니다.</param>
    /// <returns>합계 정보입니다.</returns>
    public async Task<SummaryTotals> GetTotalsAsync(DateOnly start, DateOnly end)
    {
        using var context = _contextFactory.CreateDbContext();
        var nextDay = end.AddDays(1);

        var transactions = await context.Transactions
            .Where(x => x.TransactionDate >= start && x.TransactionDate < nextDay)
            .ToListAsync();

        return new SummaryTotals
        {
            PurchaseTotal = transactions
                .Where(x => x.Kind == TransactionKind.Purchase)
                .Sum(x => x.Amount),
            SaleTotal = transactions
                .Where(x => x.Kind == TransactionKind.Sale)
                .Sum(x => x.Amount)
        };
    }
}
