using BuySales.WinForms.Data;
using BuySales.WinForms.Models;
using BuySales.WinForms.Services;
using BuySales.WinForms.Theme;

namespace BuySales.WinForms;

/// <summary>
/// 매입과 매출을 입력하고 기간별 합계를 확인하는 메인 화면입니다.
/// </summary>
public class MainForm : Form
{
    private readonly TransactionService _transactionService;
    private AppTheme _currentTheme;
    private ThemePalette _palette = ThemePalette.FromTheme(AppTheme.Dark);
    private BuySaleTransaction? _selectedTransaction;
    private bool _isLoading;

    private readonly DateTimePicker _datePicker = new();
    private readonly RadioButton _purchaseRadio = new();
    private readonly RadioButton _saleRadio = new();
    private readonly TextBox _itemNameTextBox = new();
    private readonly NumericUpDown _unitPriceInput = new();
    private readonly NumericUpDown _quantityInput = new();
    private readonly TextBox _amountTextBox = new();
    private readonly TextBox _memoTextBox = new();
    private readonly DateTimePicker _monthPicker = new();
    private readonly DataGridView _transactionsGrid = new();
    private readonly Label _dailyPurchaseLabel = new();
    private readonly Label _dailySaleLabel = new();
    private readonly Label _weeklyPurchaseLabel = new();
    private readonly Label _weeklySaleLabel = new();
    private readonly Label _monthlyPurchaseLabel = new();
    private readonly Label _monthlySaleLabel = new();
    private readonly Label _balanceLabel = new();
    private readonly Label _databasePathLabel = new();
    private readonly Button _saveButton = new();
    private readonly Button _newButton = new();
    private readonly Button _deleteButton = new();
    private readonly Button _themeButton = new();

    /// <summary>
    /// 메인 화면을 생성하고 데이터베이스와 사용자 인터페이스를 초기화합니다.
    /// </summary>
    public MainForm()
    {
        var contextFactory = new SqliteBuySalesDbContextFactory();
        DatabaseInitializer.EnsureCreated(contextFactory);
        _transactionService = new TransactionService(contextFactory);
        _currentTheme = ThemeSettings.Load();

        InitializeForm();
        BuildLayout();
        ApplyTheme();
        _ = ReloadAsync();
    }

    /// <summary>
    /// 화면의 기본 속성을 설정합니다.
    /// </summary>
    private void InitializeForm()
    {
        Text = "매입/매출 금액 관리";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1220, 780);
        Size = new Size(1380, 860);
        Font = new Font("Malgun Gothic", 12.5F, FontStyle.Regular, GraphicsUnit.Point);
    }

    /// <summary>
    /// 전체 화면 배치를 구성합니다.
    /// </summary>
    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(16)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 390));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        root.Controls.Add(CreateSidebar(), 0, 0);
        root.Controls.Add(CreateContentArea(), 1, 0);
        Controls.Add(root);
    }

    /// <summary>
    /// 왼쪽 입력 사이드바를 생성합니다.
    /// </summary>
    /// <returns>생성된 사이드바 패널입니다.</returns>
    private Control CreateSidebar()
    {
        var sidebar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 13,
            Padding = new Padding(14),
            Margin = new Padding(0, 0, 16, 0)
        };

        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));

        var title = new Label
        {
            Text = "간단 입력",
            Dock = DockStyle.Fill,
            Font = new Font(Font.FontFamily, 17F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _datePicker.Format = DateTimePickerFormat.Short;
        _datePicker.ValueChanged += async (_, _) => await RefreshSummariesFromInputDateAsync();

        _purchaseRadio.Text = "매입";
        _purchaseRadio.Checked = true;
        _purchaseRadio.Appearance = Appearance.Button;
        _purchaseRadio.TextAlign = ContentAlignment.MiddleCenter;
        _purchaseRadio.Dock = DockStyle.Fill;
        _purchaseRadio.MinimumSize = new Size(0, 42);

        _saleRadio.Text = "매출";
        _saleRadio.Appearance = Appearance.Button;
        _saleRadio.TextAlign = ContentAlignment.MiddleCenter;
        _saleRadio.Dock = DockStyle.Fill;
        _saleRadio.MinimumSize = new Size(0, 42);
        _purchaseRadio.CheckedChanged += (_, _) => ApplyTheme();
        _saleRadio.CheckedChanged += (_, _) => ApplyTheme();

        _unitPriceInput.Maximum = 999999999;
        _unitPriceInput.ThousandsSeparator = true;
        _unitPriceInput.TextAlign = HorizontalAlignment.Right;
        _unitPriceInput.ValueChanged += (_, _) => UpdateAmount();

        _quantityInput.Maximum = 999999;
        _quantityInput.DecimalPlaces = 0;
        _quantityInput.ThousandsSeparator = true;
        _quantityInput.TextAlign = HorizontalAlignment.Right;
        _quantityInput.Value = 1;
        _quantityInput.ValueChanged += (_, _) => UpdateAmount();

        _amountTextBox.ReadOnly = true;
        _amountTextBox.TextAlign = HorizontalAlignment.Right;
        ConfigureEnterNavigation();

        _saveButton.Text = "저장";
        _saveButton.Dock = DockStyle.Fill;
        _saveButton.Click += async (_, _) => await SaveAsync();

        _newButton.Text = "새 입력";
        _newButton.Dock = DockStyle.Fill;
        _newButton.Click += (_, _) => ClearInput();

        _deleteButton.Text = "삭제";
        _deleteButton.Dock = DockStyle.Fill;
        _deleteButton.Click += async (_, _) => await DeleteAsync();

        _themeButton.Dock = DockStyle.Fill;
        _themeButton.Click += (_, _) => ToggleTheme();

        sidebar.Controls.Add(title, 0, 0);
        sidebar.Controls.Add(CreateLabeledControl("날짜", _datePicker), 0, 1);
        sidebar.Controls.Add(CreateKindSelector(), 0, 2);
        sidebar.Controls.Add(CreateLabeledControl("품목", _itemNameTextBox), 0, 3);
        sidebar.Controls.Add(CreateLabeledControl("단가", _unitPriceInput), 0, 4);
        sidebar.Controls.Add(CreateLabeledControl("수량", _quantityInput), 0, 5);
        sidebar.Controls.Add(CreateLabeledControl("금액", _amountTextBox), 0, 6);
        sidebar.Controls.Add(CreateLabeledControl("메모", _memoTextBox), 0, 7);
        sidebar.Controls.Add(_saveButton, 0, 8);
        sidebar.Controls.Add(_newButton, 0, 9);
        sidebar.Controls.Add(_deleteButton, 0, 10);
        sidebar.Controls.Add(CreateDatabaseInfo(), 0, 11);
        sidebar.Controls.Add(_themeButton, 0, 12);

        return sidebar;
    }

    /// <summary>
    /// 매입과 매출 구분 선택 영역을 생성합니다.
    /// </summary>
    /// <returns>생성된 구분 선택 영역입니다.</returns>
    private Control CreateKindSelector()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 6, 0, 6)
        };

        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        panel.Controls.Add(_purchaseRadio, 0, 0);
        panel.Controls.Add(_saleRadio, 1, 0);

        return panel;
    }

    /// <summary>
    /// 오른쪽 조회와 합계 영역을 생성합니다.
    /// </summary>
    /// <returns>생성된 콘텐츠 영역입니다.</returns>
    private Control CreateContentArea()
    {
        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4
        };

        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 144));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));

        content.Controls.Add(CreateHeader(), 0, 0);
        content.Controls.Add(CreateSummaryCards(), 0, 1);
        content.Controls.Add(CreateGrid(), 0, 2);
        content.Controls.Add(CreateFooter(), 0, 3);

        return content;
    }

    /// <summary>
    /// 상단 제목과 월 선택 영역을 생성합니다.
    /// </summary>
    /// <returns>생성된 헤더 영역입니다.</returns>
    private Control CreateHeader()
    {
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };

        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 310));

        var title = new Label
        {
            Text = "매입/매출 금액 관리",
            Dock = DockStyle.Fill,
            Font = new Font(Font.FontFamily, 19F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _monthPicker.Format = DateTimePickerFormat.Custom;
        _monthPicker.CustomFormat = "yyyy년 MM월";
        _monthPicker.ShowUpDown = true;
        _monthPicker.ValueChanged += async (_, _) => await ReloadAsync();

        header.Controls.Add(title, 0, 0);
        header.Controls.Add(CreateLabeledControl("조회 월", _monthPicker), 1, 0);

        return header;
    }

    /// <summary>
    /// 일별, 주별, 월별 합계 카드 영역을 생성합니다.
    /// </summary>
    /// <returns>생성된 합계 카드 영역입니다.</returns>
    private Control CreateSummaryCards()
    {
        var cards = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0, 4, 0, 12)
        };

        cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));

        cards.Controls.Add(CreateSummaryCard("일별 합계", _dailyPurchaseLabel, _dailySaleLabel), 0, 0);
        cards.Controls.Add(CreateSummaryCard("주별 합계", _weeklyPurchaseLabel, _weeklySaleLabel), 1, 0);
        cards.Controls.Add(CreateSummaryCard("월별 합계", _monthlyPurchaseLabel, _monthlySaleLabel), 2, 0);

        return cards;
    }

    /// <summary>
    /// 개별 합계 카드를 생성합니다.
    /// </summary>
    /// <param name="title">카드 제목입니다.</param>
    /// <param name="purchaseLabel">매입 합계 라벨입니다.</param>
    /// <param name="saleLabel">매출 합계 라벨입니다.</param>
    /// <returns>생성된 합계 카드입니다.</returns>
    private Control CreateSummaryCard(string title, Label purchaseLabel, Label saleLabel)
    {
        var card = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 12, 0)
        };

        card.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        card.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        card.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        var titleLabel = new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            Font = new Font(Font.FontFamily, 13.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };

        purchaseLabel.Dock = DockStyle.Fill;
        purchaseLabel.TextAlign = ContentAlignment.MiddleLeft;
        saleLabel.Dock = DockStyle.Fill;
        saleLabel.TextAlign = ContentAlignment.MiddleLeft;

        card.Controls.Add(titleLabel, 0, 0);
        card.Controls.Add(purchaseLabel, 0, 1);
        card.Controls.Add(saleLabel, 0, 2);

        return card;
    }

    /// <summary>
    /// 거래 목록 그리드를 생성합니다.
    /// </summary>
    /// <returns>생성된 거래 목록 그리드입니다.</returns>
    private Control CreateGrid()
    {
        _transactionsGrid.Dock = DockStyle.Fill;
        _transactionsGrid.AutoGenerateColumns = false;
        _transactionsGrid.AllowUserToAddRows = false;
        _transactionsGrid.AllowUserToDeleteRows = false;
        _transactionsGrid.ReadOnly = true;
        _transactionsGrid.MultiSelect = false;
        _transactionsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _transactionsGrid.RowHeadersVisible = false;
        _transactionsGrid.RowTemplate.Height = 38;
        _transactionsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _transactionsGrid.SelectionChanged += (_, _) => LoadSelectedTransaction();

        _transactionsGrid.Columns.Add(CreateTextColumn("TransactionDate", "날짜", 110));
        _transactionsGrid.Columns.Add(CreateTextColumn("DisplayKind", "구분", 80));
        _transactionsGrid.Columns.Add(CreateTextColumn("ItemName", "품목", 180));
        _transactionsGrid.Columns.Add(CreateTextColumn("UnitPrice", "단가", 110, "N0"));
        _transactionsGrid.Columns.Add(CreateTextColumn("Quantity", "수량", 90, "N0"));
        _transactionsGrid.Columns.Add(CreateTextColumn("Amount", "금액", 130, "N0"));
        _transactionsGrid.Columns.Add(CreateTextColumn("Memo", "메모", 180));

        return _transactionsGrid;
    }

    /// <summary>
    /// Enter 키로 입력 컨트롤을 순서대로 이동하고 메모에서 저장되도록 설정합니다.
    /// </summary>
    private void ConfigureEnterNavigation()
    {
        _itemNameTextBox.KeyDown += (_, e) => MoveNextOnEnter(e, _unitPriceInput);
        _unitPriceInput.KeyDown += (_, e) => MoveNextOnEnter(e, _quantityInput);
        _quantityInput.KeyDown += (_, e) => MoveNextOnEnter(e, _amountTextBox);
        _amountTextBox.KeyDown += (_, e) => MoveNextOnEnter(e, _memoTextBox);
        _memoTextBox.KeyDown += async (_, e) => await SaveOnEnterAsync(e);
    }

    /// <summary>
    /// Enter 키가 입력되면 지정한 다음 컨트롤로 포커스를 이동합니다.
    /// </summary>
    /// <param name="e">키 입력 이벤트 정보입니다.</param>
    /// <param name="nextControl">다음으로 이동할 컨트롤입니다.</param>
    private static void MoveNextOnEnter(KeyEventArgs e, Control nextControl)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        e.SuppressKeyPress = true;
        nextControl.Focus();
    }

    /// <summary>
    /// 메모 입력 중 Enter 키가 입력되면 거래를 저장합니다.
    /// </summary>
    /// <param name="e">키 입력 이벤트 정보입니다.</param>
    /// <returns>비동기 작업입니다.</returns>
    private async Task SaveOnEnterAsync(KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        e.SuppressKeyPress = true;
        await SaveAsync();
    }

    /// <summary>
    /// 하단 차액 표시 영역을 생성합니다.
    /// </summary>
    /// <returns>생성된 하단 영역입니다.</returns>
    private Control CreateFooter()
    {
        _balanceLabel.Dock = DockStyle.Fill;
        _balanceLabel.Font = new Font(Font.FontFamily, 17F, FontStyle.Bold);
        _balanceLabel.TextAlign = ContentAlignment.MiddleRight;

        return _balanceLabel;
    }

    /// <summary>
    /// 데이터 저장 위치 안내 영역을 생성합니다.
    /// </summary>
    /// <returns>생성된 안내 영역입니다.</returns>
    private Control CreateDatabaseInfo()
    {
        _databasePathLabel.Text = $"DB 저장 위치\r\n{AppPaths.DatabasePath}";
        _databasePathLabel.Dock = DockStyle.Fill;
        _databasePathLabel.AutoEllipsis = true;
        _databasePathLabel.Font = new Font(Font.FontFamily, 9F, FontStyle.Regular);
        _databasePathLabel.TextAlign = ContentAlignment.BottomLeft;

        return _databasePathLabel;
    }

    /// <summary>
    /// 라벨과 입력 컨트롤을 함께 배치한 영역을 생성합니다.
    /// </summary>
    /// <param name="labelText">라벨 텍스트입니다.</param>
    /// <param name="control">입력 컨트롤입니다.</param>
    /// <returns>생성된 입력 영역입니다.</returns>
    private Control CreateLabeledControl(string labelText, Control control)
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(0, 4, 0, 4)
        };

        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var label = new Label
        {
            Text = labelText,
            Dock = DockStyle.Fill,
            Font = new Font(Font.FontFamily, 10F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };

        control.Dock = DockStyle.Fill;
        control.Font = new Font(Font.FontFamily, 13F, FontStyle.Regular);
        control.MinimumSize = new Size(0, 34);
        panel.Controls.Add(label, 0, 0);
        panel.Controls.Add(control, 0, 1);

        return panel;
    }

    /// <summary>
    /// 거래 목록용 텍스트 컬럼을 생성합니다.
    /// </summary>
    /// <param name="propertyName">바인딩할 속성명입니다.</param>
    /// <param name="headerText">컬럼 제목입니다.</param>
    /// <param name="width">컬럼 기본 너비입니다.</param>
    /// <param name="format">표시 형식입니다.</param>
    /// <returns>생성된 그리드 컬럼입니다.</returns>
    private static DataGridViewTextBoxColumn CreateTextColumn(
        string propertyName,
        string headerText,
        int width,
        string? format = null)
    {
        return new DataGridViewTextBoxColumn
        {
            DataPropertyName = propertyName,
            HeaderText = headerText,
            Width = width,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = format,
                Alignment = propertyName is "UnitPrice" or "Quantity" or "Amount"
                    ? DataGridViewContentAlignment.MiddleRight
                    : DataGridViewContentAlignment.MiddleLeft
            }
        };
    }

    /// <summary>
    /// 입력된 단가와 수량으로 금액을 계산합니다.
    /// </summary>
    private void UpdateAmount()
    {
        var amount = _unitPriceInput.Value * _quantityInput.Value;
        _amountTextBox.Text = amount.ToString("N0");
    }

    /// <summary>
    /// 현재 입력값을 거래 객체로 변환합니다.
    /// </summary>
    /// <returns>변환된 거래 객체입니다.</returns>
    private BuySaleTransaction BuildTransaction()
    {
        var transaction = _selectedTransaction ?? new BuySaleTransaction();
        transaction.TransactionDate = DateOnly.FromDateTime(_datePicker.Value);
        transaction.Kind = _purchaseRadio.Checked ? TransactionKind.Purchase : TransactionKind.Sale;
        transaction.ItemName = _itemNameTextBox.Text.Trim();
        transaction.UnitPrice = _unitPriceInput.Value;
        transaction.Quantity = _quantityInput.Value;
        transaction.Amount = _unitPriceInput.Value * _quantityInput.Value;
        transaction.Memo = string.IsNullOrWhiteSpace(_memoTextBox.Text)
            ? null
            : _memoTextBox.Text.Trim();

        return transaction;
    }

    /// <summary>
    /// 입력값을 검증합니다.
    /// </summary>
    /// <returns>입력값이 올바르면 true입니다.</returns>
    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(_itemNameTextBox.Text))
        {
            MessageBox.Show("품목을 입력해 주세요.", "확인", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _itemNameTextBox.Focus();
            return false;
        }

        if (_quantityInput.Value <= 0)
        {
            MessageBox.Show("수량은 0보다 커야 합니다.", "확인", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _quantityInput.Focus();
            return false;
        }

        return true;
    }

    /// <summary>
    /// 거래를 저장하고 목록과 합계를 다시 조회합니다.
    /// </summary>
    /// <returns>비동기 작업입니다.</returns>
    private async Task SaveAsync()
    {
        if (!ValidateInput())
        {
            return;
        }

        await _transactionService.SaveAsync(BuildTransaction());
        ClearInput();
        await ReloadAsync();
    }

    /// <summary>
    /// 선택된 거래를 삭제하고 목록과 합계를 다시 조회합니다.
    /// </summary>
    /// <returns>비동기 작업입니다.</returns>
    private async Task DeleteAsync()
    {
        if (_selectedTransaction is null)
        {
            MessageBox.Show("삭제할 항목을 목록에서 선택해 주세요.", "확인", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = MessageBox.Show("선택한 항목을 삭제할까요?", "삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result != DialogResult.Yes)
        {
            return;
        }

        await _transactionService.DeleteAsync(_selectedTransaction.Id);
        ClearInput();
        await ReloadAsync();
    }

    /// <summary>
    /// 입력 컨트롤을 새 입력 상태로 초기화합니다.
    /// </summary>
    private void ClearInput()
    {
        _selectedTransaction = null;
        _datePicker.Value = DateTime.Today;
        _purchaseRadio.Checked = true;
        _itemNameTextBox.Clear();
        _unitPriceInput.Value = 0;
        _quantityInput.Value = 1;
        _memoTextBox.Clear();
        UpdateAmount();
        _saveButton.Text = "저장";
        _deleteButton.Enabled = false;
        _itemNameTextBox.Focus();
    }

    /// <summary>
    /// 목록에서 선택된 거래를 입력 영역에 표시합니다.
    /// </summary>
    private void LoadSelectedTransaction()
    {
        if (_isLoading)
        {
            return;
        }

        if (_transactionsGrid.CurrentRow?.DataBoundItem is not BuySaleTransaction transaction)
        {
            return;
        }

        _selectedTransaction = transaction;
        _datePicker.Value = transaction.TransactionDate.ToDateTime(TimeOnly.MinValue);
        _purchaseRadio.Checked = transaction.Kind == TransactionKind.Purchase;
        _saleRadio.Checked = transaction.Kind == TransactionKind.Sale;
        _itemNameTextBox.Text = transaction.ItemName;
        _unitPriceInput.Value = Math.Min(_unitPriceInput.Maximum, transaction.UnitPrice);
        _quantityInput.Value = Math.Min(_quantityInput.Maximum, decimal.Truncate(transaction.Quantity));
        _memoTextBox.Text = transaction.Memo ?? string.Empty;
        UpdateAmount();
        _saveButton.Text = "수정 저장";
        _deleteButton.Enabled = true;
    }

    /// <summary>
    /// 현재 조회 월의 목록과 기간별 합계를 새로 고칩니다.
    /// </summary>
    /// <returns>비동기 작업입니다.</returns>
    private async Task ReloadAsync()
    {
        _isLoading = true;
        var month = DateOnly.FromDateTime(_monthPicker.Value);
        var transactions = await _transactionService.GetMonthlyTransactionsAsync(month);
        _transactionsGrid.DataSource = transactions;
        _transactionsGrid.ClearSelection();
        _selectedTransaction = null;
        _saveButton.Text = "저장";
        _deleteButton.Enabled = false;
        _isLoading = false;

        await UpdateSummariesAsync();
    }

    /// <summary>
    /// 입력 날짜가 바뀌었을 때 합계를 새로 고칩니다.
    /// </summary>
    /// <returns>비동기 작업입니다.</returns>
    private async Task RefreshSummariesFromInputDateAsync()
    {
        if (_isLoading)
        {
            return;
        }

        await UpdateSummariesAsync();
    }

    /// <summary>
    /// 일별, 주별, 월별 합계 라벨을 갱신합니다.
    /// </summary>
    /// <returns>비동기 작업입니다.</returns>
    private async Task UpdateSummariesAsync()
    {
        var selectedDate = DateOnly.FromDateTime(_datePicker.Value);
        var dayStart = selectedDate;
        var dayEnd = selectedDate;
        var weekStart = GetWeekStart(selectedDate);
        var weekEnd = weekStart.AddDays(6);
        var monthStart = new DateOnly(_monthPicker.Value.Year, _monthPicker.Value.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var daily = await _transactionService.GetTotalsAsync(dayStart, dayEnd);
        var weekly = await _transactionService.GetTotalsAsync(weekStart, weekEnd);
        var monthly = await _transactionService.GetTotalsAsync(monthStart, monthEnd);

        SetSummaryLabels(_dailyPurchaseLabel, _dailySaleLabel, daily);
        SetSummaryLabels(_weeklyPurchaseLabel, _weeklySaleLabel, weekly);
        SetSummaryLabels(_monthlyPurchaseLabel, _monthlySaleLabel, monthly);
        _balanceLabel.Text = $"월 차액: {monthly.Balance:N0}원";
    }

    /// <summary>
    /// 합계 카드의 매입과 매출 라벨을 설정합니다.
    /// </summary>
    /// <param name="purchaseLabel">매입 합계 라벨입니다.</param>
    /// <param name="saleLabel">매출 합계 라벨입니다.</param>
    /// <param name="totals">표시할 합계입니다.</param>
    private static void SetSummaryLabels(Label purchaseLabel, Label saleLabel, SummaryTotals totals)
    {
        purchaseLabel.Text = $"매입 {totals.PurchaseTotal:N0}원";
        saleLabel.Text = $"매출 {totals.SaleTotal:N0}원";
    }

    /// <summary>
    /// 지정한 날짜가 속한 주의 시작일을 계산합니다.
    /// </summary>
    /// <param name="date">기준 날짜입니다.</param>
    /// <returns>주의 시작일입니다.</returns>
    private static DateOnly GetWeekStart(DateOnly date)
    {
        var diff = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-diff);
    }

    /// <summary>
    /// 다크 테마와 라이트 테마를 전환합니다.
    /// </summary>
    private void ToggleTheme()
    {
        _currentTheme = _currentTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
        ThemeSettings.Save(_currentTheme);
        ApplyTheme();
    }

    /// <summary>
    /// 현재 테마를 화면 전체에 적용합니다.
    /// </summary>
    private void ApplyTheme()
    {
        _palette = ThemePalette.FromTheme(_currentTheme);
        _themeButton.Text = _currentTheme == AppTheme.Dark ? "라이트 테마" : "다크 테마";
        ApplyThemeToControl(this);
        ApplyGridTheme();
        _deleteButton.Enabled = _selectedTransaction is not null;
    }

    /// <summary>
    /// 지정한 컨트롤과 하위 컨트롤에 현재 테마를 적용합니다.
    /// </summary>
    /// <param name="control">테마를 적용할 컨트롤입니다.</param>
    private void ApplyThemeToControl(Control control)
    {
        control.BackColor = control is TextBox or NumericUpDown or DateTimePicker
            ? _palette.Card
            : _palette.Background;
        control.ForeColor = _palette.Foreground;

        if (control is TableLayoutPanel or Panel)
        {
            control.BackColor = _palette.Panel;
        }

        if (control is Button button)
        {
            button.BackColor = _palette.Accent;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font(Font.FontFamily, 12.5F, FontStyle.Bold);
            button.MinimumSize = new Size(0, 42);
        }

        if (control is RadioButton radioButton)
        {
            radioButton.BackColor = radioButton.Checked ? _palette.Accent : _palette.Card;
            radioButton.ForeColor = radioButton.Checked ? Color.White : _palette.Foreground;
            radioButton.FlatStyle = FlatStyle.Flat;
            radioButton.Font = new Font(Font.FontFamily, 12.5F, FontStyle.Bold);
        }

        foreach (Control child in control.Controls)
        {
            ApplyThemeToControl(child);
        }
    }

    /// <summary>
    /// 거래 목록 그리드에 현재 테마를 적용합니다.
    /// </summary>
    private void ApplyGridTheme()
    {
        _transactionsGrid.BackgroundColor = _palette.Background;
        _transactionsGrid.BorderStyle = BorderStyle.None;
        _transactionsGrid.EnableHeadersVisualStyles = false;
        _transactionsGrid.ColumnHeadersDefaultCellStyle.BackColor = _palette.Panel;
        _transactionsGrid.ColumnHeadersDefaultCellStyle.ForeColor = _palette.Foreground;
        _transactionsGrid.ColumnHeadersDefaultCellStyle.Font = new Font(Font.FontFamily, 12.5F, FontStyle.Bold);
        _transactionsGrid.DefaultCellStyle.Font = new Font(Font.FontFamily, 12F, FontStyle.Regular);
        _transactionsGrid.DefaultCellStyle.BackColor = _palette.Card;
        _transactionsGrid.DefaultCellStyle.ForeColor = _palette.Foreground;
        _transactionsGrid.DefaultCellStyle.SelectionBackColor = _palette.Accent;
        _transactionsGrid.DefaultCellStyle.SelectionForeColor = Color.White;
        _transactionsGrid.GridColor = _palette.Border;
    }
}
