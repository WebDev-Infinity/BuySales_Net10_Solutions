namespace BuySales.WinForms.Models;

/// <summary>
/// 일별 매입 또는 매출 입력 내역을 나타냅니다.
/// </summary>
public class BuySaleTransaction
{
    /// <summary>
    /// 거래 고유 번호를 가져오거나 설정합니다.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 거래 일자를 가져오거나 설정합니다.
    /// </summary>
    public DateOnly TransactionDate { get; set; }

    /// <summary>
    /// 매입 또는 매출 구분을 가져오거나 설정합니다.
    /// </summary>
    public TransactionKind Kind { get; set; }

    /// <summary>
    /// 화면에 표시할 한글 거래 구분명을 가져옵니다.
    /// </summary>
    public string DisplayKind => Kind == TransactionKind.Purchase ? "매입" : "매출";

    /// <summary>
    /// 품목명을 가져오거나 설정합니다.
    /// </summary>
    public string ItemName { get; set; } = string.Empty;

    /// <summary>
    /// 단가를 가져오거나 설정합니다.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 수량을 가져오거나 설정합니다.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// 금액을 가져오거나 설정합니다.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 매입 거래일 때 목록에 표시할 매입액을 가져옵니다.
    /// </summary>
    public decimal? PurchaseAmount => Kind == TransactionKind.Purchase ? Amount : null;

    /// <summary>
    /// 매출 거래일 때 목록에 표시할 매출액을 가져옵니다.
    /// </summary>
    public decimal? SaleAmount => Kind == TransactionKind.Sale ? Amount : null;

    /// <summary>
    /// 매입 거래일 때 목록에 표시할 매입액 문자열을 가져옵니다.
    /// </summary>
    public string DisplayPurchaseAmount => PurchaseAmount?.ToString("N0") ?? string.Empty;

    /// <summary>
    /// 매출 거래일 때 목록에 표시할 매출액 문자열을 가져옵니다.
    /// </summary>
    public string DisplaySaleAmount => SaleAmount?.ToString("N0") ?? string.Empty;

    /// <summary>
    /// 메모를 가져오거나 설정합니다.
    /// </summary>
    public string? Memo { get; set; }

    /// <summary>
    /// 생성 일시를 가져오거나 설정합니다.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 수정 일시를 가져오거나 설정합니다.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
