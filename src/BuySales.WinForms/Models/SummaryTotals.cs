namespace BuySales.WinForms.Models;

/// <summary>
/// 기간별 매입, 매출 합계 정보를 나타냅니다.
/// </summary>
public class SummaryTotals
{
    /// <summary>
    /// 매입 합계를 가져오거나 설정합니다.
    /// </summary>
    public decimal PurchaseTotal { get; set; }

    /// <summary>
    /// 매출 합계를 가져오거나 설정합니다.
    /// </summary>
    public decimal SaleTotal { get; set; }

    /// <summary>
    /// 매출에서 매입을 뺀 차액을 가져옵니다.
    /// </summary>
    public decimal Balance => SaleTotal - PurchaseTotal;
}
