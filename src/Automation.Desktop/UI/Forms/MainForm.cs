using Automation.Desktop.Core.Interfaces;
using Automation.Desktop.Core.Models;

namespace Automation.Desktop.UI.Forms;

public sealed class MainForm : Form
{
    private readonly IDeviceManager _deviceManager;
    private readonly IAdbService _adbService;
    private readonly BindingSource _deviceBinding = new();

    private readonly DataGridView _deviceGrid = new();
    private readonly RichTextBox _logBox = new();
    private readonly Label _statusPill = new();
    private readonly TabControl _tabs = new();

    public MainForm(IDeviceManager deviceManager, IAdbService adbService)
    {
        _deviceManager = deviceManager;
        _adbService = adbService;

        InitializeForm();
        BuildLayout();
        HookEvents();
    }

    private void InitializeForm()
    {
        Text = "Android Automation Studio";
        Width = 1500;
        Height = 920;
        MinimumSize = new Size(1200, 760);
        BackColor = Color.FromArgb(23, 27, 35);
        ForeColor = Color.Gainsboro;
        Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
        StartPosition = FormStartPosition.CenterScreen;
    }

    private void BuildLayout()
    {
        var topBar = new Panel { Dock = DockStyle.Top, Height = 54, BackColor = Color.FromArgb(30, 35, 46), Padding = new Padding(14, 8, 14, 8) };
        var title = new Label { Text = "Android Automation Studio", ForeColor = Color.WhiteSmoke, AutoSize = true, Font = new Font("Segoe UI Semibold", 12f) };
        var refreshButton = CreateButton("Refresh Devices", 155);
        refreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        refreshButton.Location = new Point(Width - 220, 10);
        refreshButton.Click += async (_, _) => await RefreshDevicesSafeAsync();

        _statusPill.Text = "● Disconnected";
        _statusPill.ForeColor = Color.LightCoral;
        _statusPill.AutoSize = true;
        _statusPill.Location = new Point(300, 16);

        topBar.Controls.Add(title);
        topBar.Controls.Add(_statusPill);
        topBar.Controls.Add(refreshButton);

        var mainSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 420,
            BackColor = Color.FromArgb(23, 27, 35),
            Panel1MinSize = 340,
            Panel2MinSize = 580
        };

        mainSplit.Panel1.Controls.Add(BuildDevicePanel());
        mainSplit.Panel2.Controls.Add(BuildRightPanel());

        Controls.Add(mainSplit);
        Controls.Add(topBar);
    }

    private Control BuildDevicePanel()
    {
        var container = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), BackColor = Color.FromArgb(23, 27, 35) };
        var card = CreateCard("Connected Devices");
        card.Dock = DockStyle.Fill;

        _deviceGrid.Dock = DockStyle.Fill;
        _deviceGrid.BackgroundColor = Color.FromArgb(34, 40, 52);
        _deviceGrid.BorderStyle = BorderStyle.None;
        _deviceGrid.GridColor = Color.FromArgb(63, 71, 87);
        _deviceGrid.MultiSelect = true;
        _deviceGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _deviceGrid.AutoGenerateColumns = false;
        _deviceGrid.AllowUserToAddRows = false;
        _deviceGrid.AllowUserToDeleteRows = false;
        _deviceGrid.RowHeadersVisible = false;
        _deviceGrid.EnableHeadersVisualStyles = false;
        _deviceGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(42, 48, 63);
        _deviceGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.WhiteSmoke;
        _deviceGrid.DefaultCellStyle.BackColor = Color.FromArgb(34, 40, 52);
        _deviceGrid.DefaultCellStyle.ForeColor = Color.Gainsboro;
        _deviceGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(64, 125, 240);
        _deviceGrid.DefaultCellStyle.SelectionForeColor = Color.White;

        _deviceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DeviceId", HeaderText = "Device ID", FillWeight = 40, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _deviceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Status", Width = 90 });
        _deviceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Model", HeaderText = "Model", FillWeight = 30, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _deviceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Health", HeaderText = "Indicator", Width = 90 });
        _deviceGrid.DataSource = _deviceBinding;

        card.Controls.Add(_deviceGrid);
        container.Controls.Add(card);
        return container;
    }

    private Control BuildRightPanel()
    {
        var outer = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 560, Panel1MinSize = 400, Panel2MinSize = 220 };

        _tabs.Dock = DockStyle.Fill;
        _tabs.Appearance = TabAppearance.Normal;
        _tabs.SizeMode = TabSizeMode.Fixed;
        _tabs.ItemSize = new Size(130, 32);
        _tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
        _tabs.DrawItem += DrawDarkTab;

        _tabs.TabPages.Add(CreateActionsTab());
        _tabs.TabPages.Add(CreatePlaceholderTab("Auto"));
        _tabs.TabPages.Add(CreatePlaceholderTab("Text Search"));
        _tabs.TabPages.Add(CreatePlaceholderTab("Restore & Reset"));
        _tabs.TabPages.Add(CreatePlaceholderTab("Random"));
        _tabs.TabPages.Add(CreatePlaceholderTab("Settings"));

        outer.Panel1.Controls.Add(_tabs);
        outer.Panel2.Controls.Add(BuildLogPanel());
        return outer;
    }

    private TabPage CreateActionsTab()
    {
        var tab = new TabPage("Actions") { BackColor = Color.FromArgb(28, 33, 44) };
        var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), BackColor = tab.BackColor, AutoScroll = true };

        foreach (var label in new[] { "Capture", "Paste Data", "Paste Password", "Run One", "Run All", "Find Click", "OCR Read", "Check Device" })
        {
            var btn = CreateButton(label, 170);
            btn.Height = 48;
            btn.Margin = new Padding(8);
            btn.Click += async (_, _) => await HandleActionAsync(label);
            flow.Controls.Add(btn);
        }

        tab.Controls.Add(flow);
        return tab;
    }

    private TabPage CreatePlaceholderTab(string name)
    {
        var tab = new TabPage(name) { BackColor = Color.FromArgb(28, 33, 44) };
        tab.Controls.Add(new Label { Text = $"{name} module", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.DarkGray, Font = new Font("Segoe UI Semibold", 11f) });
        return tab;
    }

    private Control BuildLogPanel()
    {
        var card = CreateCard("Real-time Logs");
        card.Dock = DockStyle.Fill;

        _logBox.Dock = DockStyle.Fill;
        _logBox.BackColor = Color.FromArgb(17, 21, 30);
        _logBox.ForeColor = Color.LightGray;
        _logBox.Font = new Font("Consolas", 9.5f);
        _logBox.BorderStyle = BorderStyle.None;
        _logBox.ReadOnly = true;

        card.Controls.Add(_logBox);
        return card;
    }

    private Panel CreateCard(string title)
    {
        var panel = new Panel { BackColor = Color.FromArgb(28, 33, 44), Padding = new Padding(1), Margin = new Padding(10) };
        var body = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(34, 40, 52), Padding = new Padding(10, 34, 10, 10) };
        var header = new Label { Text = title, Dock = DockStyle.Top, Height = 28, ForeColor = Color.WhiteSmoke, BackColor = Color.FromArgb(42, 48, 63), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0), Font = new Font("Segoe UI Semibold", 9.75f) };

        panel.Controls.Add(body);
        panel.Controls.Add(header);
        return body;
    }

    private Button CreateButton(string text, int width)
    {
        return new Button
        {
            Text = text,
            Width = width,
            Height = 40,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(67, 107, 196),
            ForeColor = Color.White,
            FlatAppearance = { BorderSize = 0 },
            Font = new Font("Segoe UI Semibold", 9f)
        };
    }

    private void HookEvents()
    {
        Load += async (_, _) => await RefreshDevicesSafeAsync();
        _deviceManager.DevicesUpdated += (_, devices) =>
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => BindDevices(devices));
                return;
            }
            BindDevices(devices);
        };
    }

    private async Task RefreshDevicesSafeAsync()
    {
        AppendLog("Refreshing device list...");
        try
        {
            await _deviceManager.RefreshDevicesAsync();
        }
        catch (Exception ex)
        {
            AppendLog($"[ERROR] Device refresh failed: {ex.Message}");
        }
    }

    private void BindDevices(IReadOnlyList<DeviceInfo> devices)
    {
        _deviceBinding.DataSource = devices.Select(d => new
        {
            d.DeviceId,
            d.Status,
            d.Model,
            Health = d.IsConnected ? "● Online" : "● Offline"
        }).ToList();

        var online = devices.Count(x => x.IsConnected);
        _statusPill.Text = online > 0 ? $"● {online} Online" : "● Disconnected";
        _statusPill.ForeColor = online > 0 ? Color.LightGreen : Color.LightCoral;
        AppendLog($"Devices updated: {devices.Count} total, {online} online.");
    }

    private async Task HandleActionAsync(string action)
    {
        var selected = _deviceGrid.SelectedRows.Cast<DataGridViewRow>()
            .Select(r => r.Cells[0].Value?.ToString())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .ToList();

        if (selected.Count == 0)
        {
            AppendLog("[WARN] No devices selected.");
            return;
        }

        AppendLog($"Action '{action}' started on {selected.Count} device(s).");

        if (action == "Capture")
        {
            foreach (var id in selected)
            {
                var file = Path.Combine(AppContext.BaseDirectory, $"{id}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                await _adbService.CaptureScreenAsync(id, file);
                AppendLog($"Captured screenshot: {file}");
            }
        }

        if (action == "Check Device")
        {
            AppendLog("Selected devices: " + string.Join(", ", selected));
        }
    }

    private void AppendLog(string text)
    {
        _logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {text}{Environment.NewLine}");
        _logBox.SelectionStart = _logBox.TextLength;
        _logBox.ScrollToCaret();
    }

    private void DrawDarkTab(object? sender, DrawItemEventArgs e)
    {
        var page = _tabs.TabPages[e.Index];
        var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        var back = selected ? Color.FromArgb(67, 107, 196) : Color.FromArgb(42, 48, 63);

        using var b = new SolidBrush(back);
        e.Graphics.FillRectangle(b, e.Bounds);
        TextRenderer.DrawText(e.Graphics, page.Text, new Font("Segoe UI Semibold", 9f), e.Bounds, Color.WhiteSmoke, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}
