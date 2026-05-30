namespace BuySales.WinForms.Theme;

/// <summary>
/// 버튼에 사용할 단색 아이콘 이미지를 생성합니다.
/// </summary>
public static class ButtonIconFactory
{
    /// <summary>
    /// 지정한 종류의 버튼 아이콘 이미지를 생성합니다.
    /// </summary>
    /// <param name="kind">아이콘 종류입니다.</param>
    /// <param name="color">아이콘 색상입니다.</param>
    /// <param name="size">아이콘 크기입니다.</param>
    /// <returns>생성된 아이콘 이미지입니다.</returns>
    public static Bitmap Create(ButtonIconKind kind, Color color, int size = 24)
    {
        var bitmap = new Bitmap(size, size);

        using var graphics = Graphics.FromImage(bitmap);
        using var pen = new Pen(color, Math.Max(2, size / 12))
        {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round,
            LineJoin = System.Drawing.Drawing2D.LineJoin.Round
        };
        using var brush = new SolidBrush(color);

        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.Clear(Color.Transparent);

        switch (kind)
        {
            case ButtonIconKind.Save:
                DrawSave(graphics, pen, brush, size);
                break;
            case ButtonIconKind.Add:
                DrawAdd(graphics, pen, size);
                break;
            case ButtonIconKind.Delete:
                DrawDelete(graphics, pen, size);
                break;
            case ButtonIconKind.Table:
                DrawTable(graphics, pen, size);
                break;
            case ButtonIconKind.Backup:
                DrawBackup(graphics, pen, size);
                break;
            case ButtonIconKind.Restore:
                DrawRestore(graphics, pen, size);
                break;
            case ButtonIconKind.Sun:
                DrawSun(graphics, pen, size);
                break;
            case ButtonIconKind.Moon:
                DrawMoon(graphics, pen, brush, size);
                break;
            default:
                DrawAdd(graphics, pen, size);
                break;
        }

        return bitmap;
    }

    /// <summary>
    /// 저장 아이콘을 그립니다.
    /// </summary>
    /// <param name="graphics">그래픽 컨텍스트입니다.</param>
    /// <param name="pen">외곽선 펜입니다.</param>
    /// <param name="brush">채우기 브러시입니다.</param>
    /// <param name="size">아이콘 크기입니다.</param>
    private static void DrawSave(Graphics graphics, Pen pen, Brush brush, int size)
    {
        var unit = size / 24F;
        graphics.DrawRectangle(pen, 4 * unit, 4 * unit, 16 * unit, 16 * unit);
        graphics.DrawLine(pen, 8 * unit, 4 * unit, 8 * unit, 10 * unit);
        graphics.DrawLine(pen, 8 * unit, 10 * unit, 16 * unit, 10 * unit);
        graphics.DrawRectangle(pen, 8 * unit, 14 * unit, 8 * unit, 6 * unit);
        graphics.FillRectangle(brush, 15 * unit, 6 * unit, 2 * unit, 3 * unit);
    }

    /// <summary>
    /// 추가 아이콘을 그립니다.
    /// </summary>
    /// <param name="graphics">그래픽 컨텍스트입니다.</param>
    /// <param name="pen">외곽선 펜입니다.</param>
    /// <param name="size">아이콘 크기입니다.</param>
    private static void DrawAdd(Graphics graphics, Pen pen, int size)
    {
        var unit = size / 24F;
        graphics.DrawEllipse(pen, 4 * unit, 4 * unit, 16 * unit, 16 * unit);
        graphics.DrawLine(pen, 12 * unit, 8 * unit, 12 * unit, 16 * unit);
        graphics.DrawLine(pen, 8 * unit, 12 * unit, 16 * unit, 12 * unit);
    }

    /// <summary>
    /// 삭제 아이콘을 그립니다.
    /// </summary>
    /// <param name="graphics">그래픽 컨텍스트입니다.</param>
    /// <param name="pen">외곽선 펜입니다.</param>
    /// <param name="size">아이콘 크기입니다.</param>
    private static void DrawDelete(Graphics graphics, Pen pen, int size)
    {
        var unit = size / 24F;
        graphics.DrawLine(pen, 8 * unit, 6 * unit, 16 * unit, 6 * unit);
        graphics.DrawLine(pen, 10 * unit, 4 * unit, 14 * unit, 4 * unit);
        graphics.DrawRectangle(pen, 7 * unit, 8 * unit, 10 * unit, 12 * unit);
        graphics.DrawLine(pen, 10 * unit, 11 * unit, 10 * unit, 17 * unit);
        graphics.DrawLine(pen, 14 * unit, 11 * unit, 14 * unit, 17 * unit);
    }

    /// <summary>
    /// 표 아이콘을 그립니다.
    /// </summary>
    /// <param name="graphics">그래픽 컨텍스트입니다.</param>
    /// <param name="pen">외곽선 펜입니다.</param>
    /// <param name="size">아이콘 크기입니다.</param>
    private static void DrawTable(Graphics graphics, Pen pen, int size)
    {
        var unit = size / 24F;
        graphics.DrawRectangle(pen, 4 * unit, 5 * unit, 16 * unit, 14 * unit);
        graphics.DrawLine(pen, 4 * unit, 10 * unit, 20 * unit, 10 * unit);
        graphics.DrawLine(pen, 4 * unit, 15 * unit, 20 * unit, 15 * unit);
        graphics.DrawLine(pen, 10 * unit, 5 * unit, 10 * unit, 19 * unit);
        graphics.DrawLine(pen, 15 * unit, 5 * unit, 15 * unit, 19 * unit);
    }

    /// <summary>
    /// 백업 아이콘을 그립니다.
    /// </summary>
    /// <param name="graphics">그래픽 컨텍스트입니다.</param>
    /// <param name="pen">외곽선 펜입니다.</param>
    /// <param name="size">아이콘 크기입니다.</param>
    private static void DrawBackup(Graphics graphics, Pen pen, int size)
    {
        var unit = size / 24F;
        DrawDatabase(graphics, pen, size);
        graphics.DrawLine(pen, 12 * unit, 14 * unit, 12 * unit, 6 * unit);
        graphics.DrawLine(pen, 8 * unit, 10 * unit, 12 * unit, 6 * unit);
        graphics.DrawLine(pen, 16 * unit, 10 * unit, 12 * unit, 6 * unit);
    }

    /// <summary>
    /// 복구 아이콘을 그립니다.
    /// </summary>
    /// <param name="graphics">그래픽 컨텍스트입니다.</param>
    /// <param name="pen">외곽선 펜입니다.</param>
    /// <param name="size">아이콘 크기입니다.</param>
    private static void DrawRestore(Graphics graphics, Pen pen, int size)
    {
        var unit = size / 24F;
        DrawDatabase(graphics, pen, size);
        graphics.DrawLine(pen, 12 * unit, 6 * unit, 12 * unit, 14 * unit);
        graphics.DrawLine(pen, 8 * unit, 10 * unit, 12 * unit, 14 * unit);
        graphics.DrawLine(pen, 16 * unit, 10 * unit, 12 * unit, 14 * unit);
    }

    /// <summary>
    /// 데이터베이스 모양을 그립니다.
    /// </summary>
    /// <param name="graphics">그래픽 컨텍스트입니다.</param>
    /// <param name="pen">외곽선 펜입니다.</param>
    /// <param name="size">아이콘 크기입니다.</param>
    private static void DrawDatabase(Graphics graphics, Pen pen, int size)
    {
        var unit = size / 24F;
        graphics.DrawEllipse(pen, 5 * unit, 4 * unit, 14 * unit, 5 * unit);
        graphics.DrawLine(pen, 5 * unit, 6.5F * unit, 5 * unit, 18 * unit);
        graphics.DrawLine(pen, 19 * unit, 6.5F * unit, 19 * unit, 18 * unit);
        graphics.DrawArc(pen, 5 * unit, 15.5F * unit, 14 * unit, 5 * unit, 0, 180);
    }

    /// <summary>
    /// 밝은 테마 아이콘을 그립니다.
    /// </summary>
    /// <param name="graphics">그래픽 컨텍스트입니다.</param>
    /// <param name="pen">외곽선 펜입니다.</param>
    /// <param name="size">아이콘 크기입니다.</param>
    private static void DrawSun(Graphics graphics, Pen pen, int size)
    {
        var unit = size / 24F;
        graphics.DrawEllipse(pen, 8 * unit, 8 * unit, 8 * unit, 8 * unit);
        graphics.DrawLine(pen, 12 * unit, 3 * unit, 12 * unit, 5 * unit);
        graphics.DrawLine(pen, 12 * unit, 19 * unit, 12 * unit, 21 * unit);
        graphics.DrawLine(pen, 3 * unit, 12 * unit, 5 * unit, 12 * unit);
        graphics.DrawLine(pen, 19 * unit, 12 * unit, 21 * unit, 12 * unit);
        graphics.DrawLine(pen, 5.5F * unit, 5.5F * unit, 7 * unit, 7 * unit);
        graphics.DrawLine(pen, 17 * unit, 17 * unit, 18.5F * unit, 18.5F * unit);
        graphics.DrawLine(pen, 18.5F * unit, 5.5F * unit, 17 * unit, 7 * unit);
        graphics.DrawLine(pen, 7 * unit, 17 * unit, 5.5F * unit, 18.5F * unit);
    }

    /// <summary>
    /// 어두운 테마 아이콘을 그립니다.
    /// </summary>
    /// <param name="graphics">그래픽 컨텍스트입니다.</param>
    /// <param name="pen">외곽선 펜입니다.</param>
    /// <param name="brush">채우기 브러시입니다.</param>
    /// <param name="size">아이콘 크기입니다.</param>
    private static void DrawMoon(Graphics graphics, Pen pen, Brush brush, int size)
    {
        var unit = size / 24F;
        using var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddEllipse(5 * unit, 4 * unit, 14 * unit, 16 * unit);
        path.AddEllipse(10 * unit, 2 * unit, 12 * unit, 16 * unit);
        graphics.FillPath(brush, path);
        graphics.DrawArc(pen, 5 * unit, 4 * unit, 14 * unit, 16 * unit, 80, 230);
    }
}
