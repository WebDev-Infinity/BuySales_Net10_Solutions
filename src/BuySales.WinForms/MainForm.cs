using BuySales.WinForms.Data;
using BuySales.WinForms.Models;
using BuySales.WinForms.Services;
using BuySales.WinForms.Theme;
using System.Drawing.Drawing2D;
using System.Text;

namespace BuySales.WinForms;

/// <summary>
/// 매입과 매출을 입력하고 기간별 합계를 확인하는 메인 화면입니다.
/// </summary>
public class MainForm : Form
{
    private readonly IBuySalesDbContextFactory _contextFactory;
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
    private readonly DateTimePicker _listDatePicker = new();
    private readonly DateTimePicker _monthPicker = new();
    private readonly TextBox _searchTextBox = new();
    private readonly DataGridView _transactionsGrid = new();
    private readonly Label _dailyPurchaseLabel = new();
    private readonly Label _dailySaleLabel = new();
    private readonly Label _weeklyPurchaseLabel = new();
    private readonly Label _weeklySaleLabel = new();
    private readonly Label _monthlyPurchaseLabel = new();
    private readonly Label _monthlySaleLabel = new();
    private readonly Label _balanceLabel = new();
    private readonly Button _saveButton = new();
    private readonly Button _newButton = new();
    private readonly Button _deleteButton = new();
    private readonly Button _dailyExportButton = new();
    private readonly Button _monthlyExportButton = new();
    private readonly Button _backupButton = new();
    private readonly Button _restoreButton = new();
    private readonly Button _searchButton = new();
    private readonly Button _clearSearchButton = new();
    private readonly Button _themeButton = new();
    private List<BuySaleTransaction> _loadedDailyTransactions = new();

    /// <summary>
    /// 메인 화면을 생성하고 데이터베이스와 사용자 인터페이스를 초기화합니다.
    /// </summary>
    public MainForm()
    {
        _contextFactory = new SqliteBuySalesDbContextFactory();
        DatabaseInitializer.EnsureCreated(_contextFactory);
        _transactionService = new TransactionService(_contextFactory);
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
        WindowState = FormWindowState.Maximized;
        AutoScaleMode = AutoScaleMode.Dpi;
        MinimumSize = new Size(1280, 800);
        Size = new Size(1440, 900);
        Font = new Font("Malgun Gothic", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
        ApplyApplicationIcon();
    }

    /// <summary>
    /// 실행 파일에 포함된 애플리케이션 아이콘을 현재 화면 아이콘으로 적용합니다.
    /// </summary>
    private void ApplyApplicationIcon()
    {
        var applicationIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        if (applicationIcon is not null)
        {
            Icon = applicationIcon;
        }
    }

    /// <summary>
    /// 전체 화면 배치를 구성합니다.
    /// </summary>
    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(10)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 220));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 108));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));

        ConfigureLegacyControls();
        root.Controls.Add(CreateLegacyToolbar(), 0, 0);
        root.Controls.Add(CreateLegacyInputPanel(), 0, 1);
        root.Controls.Add(CreateLegacySearchPanel(), 0, 2);
        root.Controls.Add(CreateSummaryCards(), 0, 3);
        root.Controls.Add(CreateGridGroup(), 0, 4);
        root.Controls.Add(CreateFooter(), 0, 5);
        Controls.Add(root);
    }

    /// <summary>
    /// 기존 StockV2 화면 흐름에 맞춰 재배치할 컨트롤의 속성과 이벤트를 설정합니다.
    /// </summary>
    private void ConfigureLegacyControls()
    {
        _datePicker.Format = DateTimePickerFormat.Custom;
        _datePicker.CustomFormat = "yyyy-MM-dd HH:mm";

        _listDatePicker.Format = DateTimePickerFormat.Short;
        _listDatePicker.ValueChanged += async (_, _) =>
        {
            if (_selectedTransaction is null)
            {
                _datePicker.Value = _listDatePicker.Value;
            }

            await ReloadAsync();
        };

        _monthPicker.Format = DateTimePickerFormat.Custom;
        _monthPicker.CustomFormat = "yyyy년 MM월";
        _monthPicker.ShowUpDown = true;
        _monthPicker.ValueChanged += async (_, _) => await UpdateSummariesAsync();

        _purchaseRadio.Text = "매입";
        _purchaseRadio.AutoSize = true;
        _purchaseRadio.Margin = new Padding(12, 8, 0, 0);

        _saleRadio.Text = "매출";
        _saleRadio.Checked = true;
        _saleRadio.AutoSize = true;
        _saleRadio.Margin = new Padding(12, 8, 0, 0);
        _purchaseRadio.CheckedChanged += async (_, _) => await ChangeInputKindAsync();
        _saleRadio.CheckedChanged += async (_, _) => await ChangeInputKindAsync();

        _itemNameTextBox.ImeMode = ImeMode.Hangul;
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
        _amountTextBox.TabStop = false;
        _memoTextBox.ImeMode = ImeMode.Hangul;
        ConfigureEnterNavigation();

        _newButton.Text = "등록";
        _newButton.Click += (_, _) => ClearInput();
        _saveButton.Text = "저장";
        _saveButton.Click += async (_, _) => await SaveAsync();
        _deleteButton.Text = "삭제";
        _deleteButton.Enabled = false;
        _deleteButton.Click += async (_, _) => await DeleteAsync();
        _dailyExportButton.Text = "일별 엑셀";
        _dailyExportButton.Click += async (_, _) => await ExportDailyAsync();
        _monthlyExportButton.Text = "월별 엑셀";
        _monthlyExportButton.Click += async (_, _) => await ExportMonthlyAsync();
        _backupButton.Text = "백업";
        _backupButton.Click += (_, _) => BackupDatabase();
        _restoreButton.Text = "복구";
        _restoreButton.Click += async (_, _) => await RestoreDatabaseAsync();
        _searchButton.Text = "검색";
        _searchButton.Click += (_, _) => ApplyTransactionFilter();
        _clearSearchButton.Text = "초기화";
        _clearSearchButton.Click += (_, _) =>
        {
            _searchTextBox.Clear();
            ApplyTransactionFilter();
        };
        _themeButton.Click += (_, _) => ToggleTheme();
        _searchTextBox.ImeMode = ImeMode.Hangul;
        _searchTextBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.SuppressKeyPress = true;
            ApplyTransactionFilter();
        };

        ApplyStaticButtonIcons();
    }

    /// <summary>
    /// 기존 화면의 상단 명령 버튼 영역을 생성합니다.
    /// </summary>
    /// <returns>생성된 상단 툴바입니다.</returns>
    private Control CreateLegacyToolbar()
    {
        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 3, 0, 3),
            Margin = Padding.Empty
        };

        ConfigureToolbarButton(_newButton, 104);
        ConfigureToolbarButton(_saveButton, 104);
        ConfigureToolbarButton(_deleteButton, 104);
        ConfigureToolbarButton(_dailyExportButton, 132);
        ConfigureToolbarButton(_monthlyExportButton, 132);
        ConfigureToolbarButton(_backupButton, 104);
        ConfigureToolbarButton(_restoreButton, 104);
        ConfigureToolbarButton(_themeButton, 148);

        toolbar.Controls.Add(_newButton);
        toolbar.Controls.Add(_saveButton);
        toolbar.Controls.Add(_deleteButton);
        toolbar.Controls.Add(_dailyExportButton);
        toolbar.Controls.Add(_monthlyExportButton);
        toolbar.Controls.Add(_backupButton);
        toolbar.Controls.Add(_restoreButton);
        toolbar.Controls.Add(_themeButton);

        return toolbar;
    }

    /// <summary>
    /// 툴바 버튼의 기본 크기와 여백을 설정합니다.
    /// </summary>
    /// <param name="button">설정할 버튼입니다.</param>
    /// <param name="width">버튼 너비입니다.</param>
    private static void ConfigureToolbarButton(Button button, int width)
    {
        button.Width = width;
        button.Height = 34;
        button.Margin = new Padding(0, 1, 8, 1);
    }

    /// <summary>
    /// 기존 BuySale 화면의 매입매출정보 그룹 배치를 생성합니다.
    /// </summary>
    /// <returns>생성된 입력 그룹입니다.</returns>
    private Control CreateLegacyInputPanel()
    {
        var group = new GroupBox
        {
            Text = "매입매출정보",
            Dock = DockStyle.Fill,
            Padding = new Padding(8, 4, 8, 8),
            Margin = new Padding(0, 0, 0, 8)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 470));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        layout.Controls.Add(CreateLegacyKindGroup(), 0, 0);
        layout.Controls.Add(CreateLegacyMainFields(), 1, 0);
        layout.Controls.Add(CreateLegacyMemoFields(), 2, 0);
        group.Controls.Add(layout);

        return group;
    }

    /// <summary>
    /// 매출과 매입 구분 라디오 버튼 그룹을 생성합니다.
    /// </summary>
    /// <returns>생성된 구분 그룹입니다.</returns>
    private Control CreateLegacyKindGroup()
    {
        var group = new GroupBox
        {
            Text = "구분",
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 8, 12, 8)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            Padding = new Padding(8, 14, 8, 8)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        layout.Controls.Add(_saleRadio, 0, 0);
        layout.Controls.Add(_purchaseRadio, 0, 1);
        group.Controls.Add(layout);

        return group;
    }

    /// <summary>
    /// 날짜, 품목, 수량, 단가, 금액 입력 영역을 생성합니다.
    /// </summary>
    /// <returns>생성된 주요 입력 영역입니다.</returns>
    private Control CreateLegacyMainFields()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 5,
            Margin = new Padding(0, 4, 12, 4)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 46));

        for (var row = 0; row < 5; row++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
        }

        AddLegacyInputRow(layout, 0, "날짜/시간", _datePicker, string.Empty);
        AddLegacyInputRow(layout, 1, "품목", _itemNameTextBox, string.Empty);
        AddLegacyInputRow(layout, 2, "수량", _quantityInput, "개");
        AddLegacyInputRow(layout, 3, "단가", _unitPriceInput, "원");
        AddLegacyInputRow(layout, 4, "금액", _amountTextBox, "원");

        return layout;
    }

    /// <summary>
    /// 메모 입력 영역을 생성합니다.
    /// </summary>
    /// <returns>생성된 메모 영역입니다.</returns>
    private Control CreateLegacyMemoFields()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 4, 0, 4)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 82));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _memoTextBox.Multiline = true;
        _memoTextBox.ScrollBars = ScrollBars.Vertical;
        _memoTextBox.Dock = DockStyle.Fill;

        layout.Controls.Add(CreateLegacyLabel("메모", ContentAlignment.TopRight), 0, 0);
        layout.Controls.Add(_memoTextBox, 1, 0);

        return layout;
    }

    /// <summary>
    /// 기존 화면 스타일의 한 줄 입력 행을 추가합니다.
    /// </summary>
    /// <param name="layout">행을 추가할 레이아웃입니다.</param>
    /// <param name="row">추가할 행 번호입니다.</param>
    /// <param name="labelText">라벨 텍스트입니다.</param>
    /// <param name="control">입력 컨트롤입니다.</param>
    /// <param name="unitText">단위 텍스트입니다.</param>
    private void AddLegacyInputRow(TableLayoutPanel layout, int row, string labelText, Control control, string unitText)
    {
        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(0, 4, 8, 4);
        control.Font = new Font(Font.FontFamily, 9.5F, FontStyle.Regular);
        control.MinimumSize = new Size(0, 28);

        layout.Controls.Add(CreateLegacyLabel(labelText, ContentAlignment.MiddleRight), 0, row);
        layout.Controls.Add(control, 1, row);
        layout.Controls.Add(CreateLegacyLabel(unitText, ContentAlignment.MiddleLeft), 2, row);
    }

    /// <summary>
    /// 기존 화면 스타일의 라벨을 생성합니다.
    /// </summary>
    /// <param name="text">라벨 텍스트입니다.</param>
    /// <param name="alignment">텍스트 정렬 방식입니다.</param>
    /// <returns>생성된 라벨입니다.</returns>
    private Label CreateLegacyLabel(string text, ContentAlignment alignment)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = alignment,
            Font = new Font(Font.FontFamily, 9.5F, FontStyle.Bold),
            Margin = new Padding(0, 4, 8, 4)
        };
    }

    /// <summary>
    /// 기존 화면의 조회 날짜와 검색어 입력 영역을 생성합니다.
    /// </summary>
    /// <returns>생성된 조회 영역입니다.</returns>
    private Control CreateLegacySearchPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = 1,
            Padding = new Padding(0, 2, 0, 6),
            Margin = Padding.Empty
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 460));

        _listDatePicker.Dock = DockStyle.Top;
        _searchTextBox.Dock = DockStyle.Top;
        _listDatePicker.Margin = new Padding(0, 12, 4, 0);
        _searchTextBox.Margin = new Padding(0, 12, 4, 0);
        ConfigureSearchButton(_searchButton);
        ConfigureSearchButton(_clearSearchButton);

        panel.Controls.Add(_listDatePicker, 0, 0);
        panel.Controls.Add(_searchTextBox, 1, 0);
        panel.Controls.Add(_searchButton, 2, 0);
        panel.Controls.Add(_clearSearchButton, 3, 0);
        panel.Controls.Add(CreateSearchMonthControl(), 4, 0);

        return panel;
    }

    /// <summary>
    /// 검색 줄에 들어가는 월별 합계 선택 영역을 인라인으로 생성합니다.
    /// </summary>
    /// <returns>생성된 월 선택 영역입니다.</returns>
    private Control CreateSearchMonthControl()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 1,
            Height = 30,
            Margin = new Padding(8, 12, 0, 0)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var label = new Label
        {
            Text = "월별 합계",
            Dock = DockStyle.Fill,
            Font = new Font(Font.FontFamily, 9.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleRight,
            Margin = new Padding(0, 0, 8, 0)
        };

        _monthPicker.Dock = DockStyle.Fill;
        _monthPicker.Font = new Font(Font.FontFamily, 9.5F, FontStyle.Regular);
        _monthPicker.Margin = Padding.Empty;
        panel.Controls.Add(label, 0, 0);
        panel.Controls.Add(_monthPicker, 1, 0);

        return panel;
    }

    /// <summary>
    /// 검색 영역 버튼의 기본 배치를 설정합니다.
    /// </summary>
    /// <param name="button">설정할 버튼입니다.</param>
    private static void ConfigureSearchButton(Button button)
    {
        button.Dock = DockStyle.Top;
        button.Margin = new Padding(4, 12, 0, 0);
        button.MinimumSize = new Size(0, 0);
        button.Height = 30;
    }

    /// <summary>
    /// 거래 목록 그리드를 기존 화면처럼 그룹 박스 안에 배치합니다.
    /// </summary>
    /// <returns>생성된 목록 그룹입니다.</returns>
    private Control CreateGridGroup()
    {
        var group = new GroupBox
        {
            Text = "매입매출목록",
            Dock = DockStyle.Fill,
            Padding = new Padding(8, 4, 8, 8),
            Margin = new Padding(0, 0, 0, 6)
        };

        group.Controls.Add(CreateGrid());
        return group;
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
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));

        var title = new Label
        {
            Text = "간단 입력",
            Dock = DockStyle.Fill,
            Font = new Font(Font.FontFamily, 17F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _datePicker.Format = DateTimePickerFormat.Short;
        _datePicker.ValueChanged += async (_, _) => await ReloadAsync();

        _purchaseRadio.Text = "매입";
        _purchaseRadio.Appearance = Appearance.Button;
        _purchaseRadio.TextAlign = ContentAlignment.MiddleCenter;
        _purchaseRadio.Dock = DockStyle.Fill;
        _purchaseRadio.MinimumSize = new Size(0, 42);

        _saleRadio.Text = "매출";
        _saleRadio.Checked = true;
        _saleRadio.Appearance = Appearance.Button;
        _saleRadio.TextAlign = ContentAlignment.MiddleCenter;
        _saleRadio.Dock = DockStyle.Fill;
        _saleRadio.MinimumSize = new Size(0, 42);
        _purchaseRadio.CheckedChanged += async (_, _) => await ChangeInputKindAsync();
        _saleRadio.CheckedChanged += async (_, _) => await ChangeInputKindAsync();

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
        _amountTextBox.TabStop = false;
        ConfigureEnterNavigation();

        _saveButton.Text = "저장";
        _saveButton.Dock = DockStyle.Fill;
        _saveButton.Click += async (_, _) => await SaveAsync();

        _newButton.Text = "새 입력";
        _newButton.Dock = DockStyle.Fill;
        _newButton.Click += (_, _) => ClearInput();

        _deleteButton.Text = "삭제";
        _deleteButton.Dock = DockStyle.Fill;
        _deleteButton.Enabled = false;
        _deleteButton.Click += async (_, _) => await DeleteAsync();

        _dailyExportButton.Text = "일별 엑셀";
        _dailyExportButton.Dock = DockStyle.Fill;
        _dailyExportButton.Click += async (_, _) => await ExportDailyAsync();

        _monthlyExportButton.Text = "월별 엑셀";
        _monthlyExportButton.Dock = DockStyle.Fill;
        _monthlyExportButton.Click += async (_, _) => await ExportMonthlyAsync();

        _backupButton.Text = "데이터 백업";
        _backupButton.Dock = DockStyle.Fill;
        _backupButton.Click += (_, _) => BackupDatabase();

        _restoreButton.Text = "데이터 복구";
        _restoreButton.Dock = DockStyle.Fill;
        _restoreButton.Click += async (_, _) => await RestoreDatabaseAsync();

        _themeButton.Dock = DockStyle.Fill;
        _themeButton.Click += (_, _) => ToggleTheme();
        ApplyStaticButtonIcons();

        sidebar.Controls.Add(CreateTitleRow(title, _newButton), 0, 0);
        sidebar.Controls.Add(CreateLabeledControl("날짜", _datePicker), 0, 1);
        sidebar.Controls.Add(CreateKindSelector(), 0, 2);
        sidebar.Controls.Add(CreateLabeledControl("품목", _itemNameTextBox), 0, 3);
        sidebar.Controls.Add(CreateLabeledControl("단가", _unitPriceInput), 0, 4);
        sidebar.Controls.Add(CreateLabeledControl("수량", _quantityInput), 0, 5);
        sidebar.Controls.Add(CreateLabeledControl("금액", _amountTextBox), 0, 6);
        sidebar.Controls.Add(CreateLabeledControl("메모", _memoTextBox), 0, 7);
        sidebar.Controls.Add(CreateButtonRow(_saveButton, _deleteButton), 0, 8);
        sidebar.Controls.Add(CreateButtonRow(_dailyExportButton, _monthlyExportButton), 0, 9);
        sidebar.Controls.Add(CreateButtonRow(_backupButton, _restoreButton), 0, 10);
        sidebar.Controls.Add(_themeButton, 0, 12);

        return sidebar;
    }

    /// <summary>
    /// 제목과 새 입력 버튼을 같은 행에 절반 너비로 배치한 영역을 생성합니다.
    /// </summary>
    /// <param name="title">왼쪽에 배치할 제목 라벨입니다.</param>
    /// <param name="newButton">오른쪽에 배치할 새 입력 버튼입니다.</param>
    /// <returns>생성된 제목 행 영역입니다.</returns>
    private static Control CreateTitleRow(Label title, Button newButton)
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty
        };

        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        title.Margin = new Padding(0, 0, 4, 0);
        newButton.Margin = new Padding(4, 3, 0, 3);
        panel.Controls.Add(title, 0, 0);
        panel.Controls.Add(newButton, 1, 0);

        return panel;
    }

    /// <summary>
    /// 두 개의 버튼을 같은 행에 절반 너비로 배치한 영역을 생성합니다.
    /// </summary>
    /// <param name="leftButton">왼쪽에 배치할 버튼입니다.</param>
    /// <param name="rightButton">오른쪽에 배치할 버튼입니다.</param>
    /// <returns>생성된 버튼 행 영역입니다.</returns>
    private static Control CreateButtonRow(Button leftButton, Button rightButton)
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty
        };

        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        leftButton.Margin = new Padding(0, 3, 4, 3);
        rightButton.Margin = new Padding(4, 3, 0, 3);
        panel.Controls.Add(leftButton, 0, 0);
        panel.Controls.Add(rightButton, 1, 0);

        return panel;
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
        panel.Controls.Add(_saleRadio, 0, 0);
        panel.Controls.Add(_purchaseRadio, 1, 0);

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

        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
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
        _monthPicker.ValueChanged += async (_, _) => await UpdateSummariesAsync();

        header.Controls.Add(title, 0, 0);
        header.Controls.Add(CreateLabeledControl("월별 합계", _monthPicker), 1, 0);

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
            Margin = new Padding(0, 0, 0, 4)
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
            Font = new Font(Font.FontFamily, 10.5F, FontStyle.Bold),
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
        _transactionsGrid.RowTemplate.Height = 32;
        _transactionsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        _transactionsGrid.ColumnHeadersHeight = 38;
        _transactionsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _transactionsGrid.CellClick += (_, _) => LoadSelectedTransaction();
        _transactionsGrid.DataBindingComplete += (_, _) => ClearGridSelection();

        _transactionsGrid.Columns.Add(CreateTextColumn("TransactionDate", "날짜", 110));
        _transactionsGrid.Columns.Add(CreateTextColumn("DisplayKind", "구분", 80));
        _transactionsGrid.Columns.Add(CreateTextColumn("ItemName", "품목", 180));
        _transactionsGrid.Columns.Add(CreateTextColumn("Quantity", "수량", 90, "N0"));
        _transactionsGrid.Columns.Add(CreateTextColumn("UnitPrice", "단가", 110, "N0"));
        _transactionsGrid.Columns.Add(CreateTextColumn("DisplayPurchaseAmount", "매입액", 130));
        _transactionsGrid.Columns.Add(CreateTextColumn("DisplaySaleAmount", "매출액", 130));
        _transactionsGrid.Columns.Add(CreateTextColumn("Memo", "메모", 180));

        return _transactionsGrid;
    }

    /// <summary>
    /// Enter 키로 품목, 수량, 단가 순서로 이동하고 단가 입력 후 저장되도록 설정합니다.
    /// </summary>
    private void ConfigureEnterNavigation()
    {
        _itemNameTextBox.KeyDown += (_, e) => MoveNextOnEnter(e, _quantityInput);
        _quantityInput.KeyDown += (_, e) => MoveNextOnEnter(e, _unitPriceInput);
        _unitPriceInput.KeyDown += async (_, e) => await SaveOnEnterAsync(e);
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
        UpdateAmount();
        await SaveAsync();
    }

    /// <summary>
    /// 선택한 날짜의 거래 목록을 엑셀에서 열 수 있는 CSV 파일로 내보냅니다.
    /// </summary>
    /// <returns>비동기 작업입니다.</returns>
    private async Task ExportDailyAsync()
    {
        var selectedDate = DateOnly.FromDateTime(_datePicker.Value);
        var transactions = await _transactionService.GetDailyTransactionsAsync(selectedDate);
        await ExportTransactionsAsync(transactions, $"일별_{selectedDate:yyyyMMdd}");
    }

    /// <summary>
    /// 선택한 월의 거래 목록을 엑셀에서 열 수 있는 CSV 파일로 내보냅니다.
    /// </summary>
    /// <returns>비동기 작업입니다.</returns>
    private async Task ExportMonthlyAsync()
    {
        var selectedMonth = DateOnly.FromDateTime(_monthPicker.Value);
        var transactions = await _transactionService.GetMonthlyTransactionsAsync(selectedMonth);
        await ExportTransactionsAsync(transactions, $"월별_{selectedMonth:yyyyMM}");
    }

    /// <summary>
    /// 사용자가 선택한 폴더에 현재 SQLite 데이터베이스 파일을 백업합니다.
    /// </summary>
    private void BackupDatabase()
    {
        if (!File.Exists(AppPaths.DatabasePath))
        {
            MessageBox.Show("백업할 데이터 파일이 없습니다.", "데이터 백업", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new FolderBrowserDialog
        {
            Description = "백업 파일을 저장할 폴더를 선택해 주세요.",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var fileName = $"BuySalesNet10_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";
        var backupPath = Path.Combine(dialog.SelectedPath, fileName);
        File.Copy(AppPaths.DatabasePath, backupPath, true);

        MessageBox.Show($"데이터 백업이 완료되었습니다.\r\n\r\n{backupPath}", "데이터 백업", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    /// <summary>
    /// 사용자가 선택한 SQLite 백업 파일로 현재 데이터베이스를 덮어씁니다.
    /// </summary>
    /// <returns>비동기 작업입니다.</returns>
    private async Task RestoreDatabaseAsync()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "복구할 백업 파일 선택",
            Filter = "SQLite 백업 파일 (*.db)|*.db|모든 파일 (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        if (Path.GetFullPath(dialog.FileName).Equals(Path.GetFullPath(AppPaths.DatabasePath), StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("현재 사용 중인 데이터 파일은 복구 파일로 선택할 수 없습니다.", "데이터 복구", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = MessageBox.Show(
            "현재 데이터가 선택한 백업 파일로 덮어쓰기 됩니다.\r\n계속 진행할까요?",
            "데이터 복구 확인",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes)
        {
            return;
        }

        _transactionsGrid.DataSource = null;
        _selectedTransaction = null;
        File.Copy(dialog.FileName, AppPaths.DatabasePath, true);
        DatabaseInitializer.EnsureCreated(_contextFactory);
        ClearInput();
        await ReloadAsync();

        MessageBox.Show("데이터 복구가 완료되었습니다.", "데이터 복구", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    /// <summary>
    /// 거래 목록을 사용자가 선택한 CSV 파일로 저장합니다.
    /// </summary>
    /// <param name="transactions">내보낼 거래 목록입니다.</param>
    /// <param name="fileNamePrefix">기본 파일명 접두사입니다.</param>
    /// <returns>비동기 작업입니다.</returns>
    private async Task ExportTransactionsAsync(IReadOnlyCollection<BuySaleTransaction> transactions, string fileNamePrefix)
    {
        if (transactions.Count == 0)
        {
            MessageBox.Show("내보낼 거래 내역이 없습니다.", "엑셀 내보내기", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Title = "엑셀 내보내기",
            Filter = "Excel CSV 파일 (*.csv)|*.csv",
            FileName = $"매입매출_{fileNamePrefix}.csv",
            AddExtension = true,
            DefaultExt = "csv"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        await WriteTransactionsCsvAsync(dialog.FileName, transactions);
        MessageBox.Show("엑셀 파일로 내보냈습니다.", "엑셀 내보내기", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    /// <summary>
    /// 거래 목록을 UTF-8 BOM CSV 파일로 저장합니다.
    /// </summary>
    /// <param name="filePath">저장할 파일 경로입니다.</param>
    /// <param name="transactions">저장할 거래 목록입니다.</param>
    /// <returns>비동기 작업입니다.</returns>
    private static async Task WriteTransactionsCsvAsync(string filePath, IEnumerable<BuySaleTransaction> transactions)
    {
        await using var writer = new StreamWriter(filePath, false, new UTF8Encoding(true));
        await writer.WriteLineAsync("날짜,구분,품목,단가,수량,매입액,매출액,메모");

        foreach (var transaction in transactions)
        {
            var line = string.Join(",",
                EscapeCsv(transaction.TransactionDate.ToString("yyyy-MM-dd")),
                EscapeCsv(transaction.DisplayKind),
                EscapeCsv(transaction.ItemName),
                EscapeCsv(transaction.UnitPrice.ToString("0.##")),
                EscapeCsv(decimal.Truncate(transaction.Quantity).ToString("0")),
                EscapeCsv(transaction.PurchaseAmount?.ToString("0.##") ?? string.Empty),
                EscapeCsv(transaction.SaleAmount?.ToString("0.##") ?? string.Empty),
                EscapeCsv(transaction.Memo ?? string.Empty));

            await writer.WriteLineAsync(line);
        }
    }

    /// <summary>
    /// CSV 셀 값을 안전하게 저장할 수 있도록 이스케이프합니다.
    /// </summary>
    /// <param name="value">CSV 셀 값입니다.</param>
    /// <returns>이스케이프된 CSV 셀 값입니다.</returns>
    private static string EscapeCsv(string value)
    {
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    /// <summary>
    /// 하단 차액 표시 영역을 생성합니다.
    /// </summary>
    /// <returns>생성된 하단 영역입니다.</returns>
    private Control CreateFooter()
    {
        _balanceLabel.Dock = DockStyle.Fill;
        _balanceLabel.Font = new Font(Font.FontFamily, 13F, FontStyle.Bold);
        _balanceLabel.TextAlign = ContentAlignment.MiddleRight;

        return _balanceLabel;
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
            Font = new Font(Font.FontFamily, GetInputLabelFontSizeByDpi(), FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };

        control.Dock = DockStyle.Fill;
        control.Font = new Font(Font.FontFamily, 9.5F, FontStyle.Regular);
        control.MinimumSize = new Size(0, 28);
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
                Alignment = propertyName is "UnitPrice" or "Quantity" or "Amount" or "DisplayPurchaseAmount" or "DisplaySaleAmount"
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

        if (_unitPriceInput.Value < 1)
        {
            MessageBox.Show("단가는 1원 이상 입력해 주세요.", "확인", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _unitPriceInput.Focus();
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
        UpdateAmount();

        if (!ValidateInput())
        {
            return;
        }

        await _transactionService.SaveAsync(BuildTransaction());
        _listDatePicker.Value = _datePicker.Value;
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
        ResetInputForNewTransaction();
        _saveButton.Text = "저장";
        _deleteButton.Enabled = false;
        ClearGridSelection();
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
    /// 매입 또는 매출 입력 구분이 바뀌면 선택 버튼 테마를 다시 적용합니다.
    /// </summary>
    /// <returns>비동기 작업입니다.</returns>
    private async Task ChangeInputKindAsync()
    {
        ApplyTheme();

        if (_isLoading)
        {
            return;
        }

        await UpdateSummariesAsync();
    }

    /// <summary>
    /// 현재 조회 날짜의 전체 매입과 매출 목록, 기간별 합계를 새로 고칩니다.
    /// </summary>
    /// <returns>비동기 작업입니다.</returns>
    private async Task ReloadAsync()
    {
        _isLoading = true;
        var selectedDate = DateOnly.FromDateTime(_listDatePicker.Value);
        _loadedDailyTransactions = await _transactionService.GetDailyTransactionsAsync(selectedDate);
        ApplyTransactionFilter();
        ClearGridSelection();
        _selectedTransaction = null;
        if (_datePicker.Value.Date != _listDatePicker.Value.Date)
        {
            _datePicker.Value = _listDatePicker.Value;
        }

        ResetInputForNewTransaction();
        _saveButton.Text = "저장";
        _deleteButton.Enabled = false;
        _isLoading = false;

        await UpdateSummariesAsync();
    }

    /// <summary>
    /// 거래 목록의 자동 선택을 해제합니다.
    /// </summary>
    private void ClearGridSelection()
    {
        _transactionsGrid.ClearSelection();
        _transactionsGrid.CurrentCell = null;
    }

    /// <summary>
    /// 선택 날짜는 유지하면서 새 입력 상태로 입력값을 초기화합니다.
    /// </summary>
    private void ResetInputForNewTransaction()
    {
        _itemNameTextBox.Clear();
        _unitPriceInput.Value = 0;
        _quantityInput.Value = 1;
        _memoTextBox.Clear();
        UpdateAmount();
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
        var selectedDate = DateOnly.FromDateTime(_listDatePicker.Value);
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
        _balanceLabel.Text = $"일 매입: {daily.PurchaseTotal:N0}원   일 매출: {daily.SaleTotal:N0}원   일 차액: {daily.Balance:N0}원";
    }

    /// <summary>
    /// 현재 조회된 일자 목록에 검색어 필터를 적용합니다.
    /// </summary>
    private void ApplyTransactionFilter()
    {
        var query = _searchTextBox.Text.Trim();
        var transactions = string.IsNullOrWhiteSpace(query)
            ? _loadedDailyTransactions
            : _loadedDailyTransactions
                .Where(x =>
                    x.ItemName.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                    x.DisplayKind.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                    (x.Memo?.Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false))
                .ToList();

        _transactionsGrid.DataSource = transactions;
        ClearGridSelection();
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
    /// 테마와 무관하게 고정으로 사용하는 버튼 아이콘을 적용합니다.
    /// </summary>
    private void ApplyStaticButtonIcons()
    {
        SetButtonIcon(_saveButton, ButtonIconKind.Save);
        SetButtonIcon(_newButton, ButtonIconKind.Add);
        SetButtonIcon(_deleteButton, ButtonIconKind.Delete);
        SetButtonIcon(_dailyExportButton, ButtonIconKind.Table);
        SetButtonIcon(_monthlyExportButton, ButtonIconKind.Table);
        SetButtonIcon(_backupButton, ButtonIconKind.Backup);
        SetButtonIcon(_restoreButton, ButtonIconKind.Restore);
        SetButtonIcon(_searchButton, ButtonIconKind.Search);
        SetButtonIcon(_clearSearchButton, ButtonIconKind.Refresh);
    }

    /// <summary>
    /// 지정한 버튼에 아이콘 이미지를 적용합니다.
    /// </summary>
    /// <param name="button">아이콘을 적용할 버튼입니다.</param>
    /// <param name="kind">아이콘 종류입니다.</param>
    private static void SetButtonIcon(Button button, ButtonIconKind kind)
    {
        button.Image?.Dispose();
        button.Image = ButtonIconFactory.Create(kind, Color.White, 18);
        button.ImageAlign = ContentAlignment.MiddleLeft;
        button.TextAlign = ContentAlignment.MiddleCenter;
        button.TextImageRelation = TextImageRelation.ImageBeforeText;
        button.Padding = new Padding(6, 0, 6, 0);
    }

    /// <summary>
    /// 현재 테마를 화면 전체에 적용합니다.
    /// </summary>
    private void ApplyTheme()
    {
        _palette = ThemePalette.FromTheme(_currentTheme);
        _themeButton.Text = _currentTheme == AppTheme.Dark ? "라이트 테마" : "다크 테마";
        SetButtonIcon(_themeButton, _currentTheme == AppTheme.Dark ? ButtonIconKind.Sun : ButtonIconKind.Moon);
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
            button.BackColor = GetButtonBackColor(button);
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = GetButtonHoverColor(button);
            button.FlatAppearance.MouseDownBackColor = GetButtonPressedColor(button);
            button.Font = new Font(Font.FontFamily, GetButtonFontSizeByDpi(), FontStyle.Bold);
            button.MinimumSize = new Size(0, 30);
            ApplyRoundedButton(button);
        }

        if (control is RadioButton radioButton)
        {
            radioButton.BackColor = radioButton.Checked ? _palette.Accent : _palette.Card;
            radioButton.ForeColor = radioButton.Checked ? Color.White : _palette.Foreground;
            radioButton.FlatStyle = FlatStyle.Flat;
            radioButton.Font = new Font(Font.FontFamily, 9.5F, FontStyle.Bold);
        }

        foreach (Control child in control.Controls)
        {
            ApplyThemeToControl(child);
        }
    }

    /// <summary>
    /// 버튼 모서리를 살짝 둥글게 처리합니다.
    /// </summary>
    /// <param name="button">둥근 모서리를 적용할 버튼입니다.</param>
    private static void ApplyRoundedButton(Button button)
    {
        UpdateRoundedButtonRegion(button);
        button.Resize -= RoundedButtonResize;
        button.Resize += RoundedButtonResize;
    }

    /// <summary>
    /// 버튼 크기가 바뀔 때 둥근 모서리 영역을 다시 계산합니다.
    /// </summary>
    /// <param name="sender">크기가 바뀐 버튼입니다.</param>
    /// <param name="e">이벤트 정보입니다.</param>
    private static void RoundedButtonResize(object? sender, EventArgs e)
    {
        if (sender is Button button)
        {
            UpdateRoundedButtonRegion(button);
        }
    }

    /// <summary>
    /// 현재 버튼 크기에 맞춰 둥근 사각형 영역을 적용합니다.
    /// </summary>
    /// <param name="button">영역을 적용할 버튼입니다.</param>
    private static void UpdateRoundedButtonRegion(Button button)
    {
        if (button.Width <= 0 || button.Height <= 0)
        {
            return;
        }

        button.Region?.Dispose();
        using var path = CreateRoundedRectanglePath(new Rectangle(0, 0, button.Width, button.Height), 6);
        button.Region = new Region(path);
    }

    /// <summary>
    /// 둥근 사각형 그래픽 경로를 생성합니다.
    /// </summary>
    /// <param name="bounds">사각형 영역입니다.</param>
    /// <param name="radius">모서리 반지름입니다.</param>
    /// <returns>생성된 그래픽 경로입니다.</returns>
    private static GraphicsPath CreateRoundedRectanglePath(Rectangle bounds, int radius)
    {
        var diameter = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }

    /// <summary>
    /// 버튼의 역할과 현재 테마에 맞는 배경색을 가져옵니다.
    /// </summary>
    /// <param name="button">색상을 결정할 버튼입니다.</param>
    /// <returns>버튼 배경색입니다.</returns>
    private Color GetButtonBackColor(Button button)
    {
        if (button == _deleteButton)
        {
            return _currentTheme == AppTheme.Light
                ? Color.FromArgb(220, 88, 88)
                : Color.FromArgb(190, 84, 84);
        }

        if (button == _saveButton || button == _searchButton)
        {
            return _currentTheme == AppTheme.Light
                ? Color.FromArgb(74, 122, 201)
                : Color.FromArgb(56, 139, 189);
        }

        return _currentTheme == AppTheme.Light
            ? Color.FromArgb(100, 116, 139)
            : Color.FromArgb(71, 85, 105);
    }

    /// <summary>
    /// 버튼에 마우스를 올렸을 때 사용할 배경색을 가져옵니다.
    /// </summary>
    /// <param name="button">색상을 결정할 버튼입니다.</param>
    /// <returns>마우스 오버 배경색입니다.</returns>
    private Color GetButtonHoverColor(Button button)
    {
        if (button == _deleteButton)
        {
            return _currentTheme == AppTheme.Light
                ? Color.FromArgb(200, 72, 72)
                : Color.FromArgb(170, 68, 68);
        }

        if (button == _saveButton || button == _searchButton)
        {
            return _currentTheme == AppTheme.Light
                ? Color.FromArgb(60, 104, 180)
                : Color.FromArgb(45, 122, 169);
        }

        return _currentTheme == AppTheme.Light
            ? Color.FromArgb(71, 85, 105)
            : Color.FromArgb(51, 65, 85);
    }

    /// <summary>
    /// 버튼을 눌렀을 때 사용할 배경색을 가져옵니다.
    /// </summary>
    /// <param name="button">색상을 결정할 버튼입니다.</param>
    /// <returns>마우스 다운 배경색입니다.</returns>
    private Color GetButtonPressedColor(Button button)
    {
        if (button == _deleteButton)
        {
            return _currentTheme == AppTheme.Light
                ? Color.FromArgb(180, 56, 56)
                : Color.FromArgb(150, 56, 56);
        }

        if (button == _saveButton || button == _searchButton)
        {
            return _currentTheme == AppTheme.Light
                ? Color.FromArgb(48, 88, 158)
                : Color.FromArgb(35, 105, 150);
        }

        return _currentTheme == AppTheme.Light
            ? Color.FromArgb(51, 65, 85)
            : Color.FromArgb(30, 41, 59);
    }

    /// <summary>
    /// 현재 모니터 DPI 배율에 맞는 버튼 글자 크기를 가져옵니다.
    /// </summary>
    /// <returns>DPI 기준으로 조정된 버튼 글자 크기입니다.</returns>
    private float GetButtonFontSizeByDpi()
    {
        return DeviceDpi >= 144 ? 8.5F : 9.5F;
    }

    /// <summary>
    /// 현재 모니터 DPI 배율에 맞는 입력 라벨 글자 크기를 가져옵니다.
    /// </summary>
    /// <returns>DPI 기준으로 조정된 입력 라벨 글자 크기입니다.</returns>
    private float GetInputLabelFontSizeByDpi()
    {
        return DeviceDpi >= 144 ? 8.5F : 9.5F;
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
        _transactionsGrid.ColumnHeadersDefaultCellStyle.Font = new Font(Font.FontFamily, 9.5F, FontStyle.Bold);
        _transactionsGrid.ColumnHeadersDefaultCellStyle.Padding = Padding.Empty;
        _transactionsGrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        _transactionsGrid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
        _transactionsGrid.DefaultCellStyle.Font = new Font(Font.FontFamily, 9.5F, FontStyle.Regular);
        _transactionsGrid.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
        _transactionsGrid.DefaultCellStyle.BackColor = _palette.Card;
        _transactionsGrid.DefaultCellStyle.ForeColor = _palette.Foreground;
        _transactionsGrid.DefaultCellStyle.SelectionBackColor = _palette.Accent;
        _transactionsGrid.DefaultCellStyle.SelectionForeColor = Color.White;
        _transactionsGrid.GridColor = _palette.Border;
    }
}
