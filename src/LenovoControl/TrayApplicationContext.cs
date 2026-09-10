using System.ComponentModel;
using System.Drawing;

namespace LenovoControl;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly ContextMenuStrip _menu;
    private readonly Icon _applicationIcon;

    private readonly ToolStripMenuItem _batteryItem;
    private readonly ToolStripMenuItem _balancedItem;
    private readonly ToolStripMenuItem _performanceItem;
    private readonly ToolStripMenuItem _startupItem;

    public TrayApplicationContext()
    {
        _applicationIcon =
            TrayIconFactory.Create();

        _batteryItem =
            new ToolStripMenuItem("Battery");

        _balancedItem =
            new ToolStripMenuItem("Balanced");

        _performanceItem =
            new ToolStripMenuItem("Performance");

        _startupItem =
            new ToolStripMenuItem("Avvia con Windows");

        _batteryItem.Click += (_, _) =>
            ActivateProfile(PowerProfile.Battery);

        _balancedItem.Click += (_, _) =>
            ActivateProfile(PowerProfile.Balanced);

        _performanceItem.Click += (_, _) =>
            ActivateProfile(PowerProfile.Performance);

        _startupItem.Click += (_, _) =>
            ToggleStartup();

        var exitItem =
            new ToolStripMenuItem("Esci");

        exitItem.Click += (_, _) =>
            ExitThread();

        _menu = new ContextMenuStrip
        {
            ShowCheckMargin = true,
            ShowImageMargin = false
        };

        _menu.Items.Add(_batteryItem);
        _menu.Items.Add(_balancedItem);
        _menu.Items.Add(_performanceItem);

        _menu.Items.Add(
            new ToolStripSeparator());

        _menu.Items.Add(_startupItem);

        _menu.Items.Add(
            new ToolStripSeparator());

        _menu.Items.Add(exitItem);

        _menu.Opening += (_, _) =>
            RefreshState();

        _trayIcon = new NotifyIcon
        {
            Icon = _applicationIcon,
            ContextMenuStrip = _menu,
            Visible = true,
            Text = "Lenovo Control"
        };

        RefreshState();
    }

    private void ActivateProfile(PowerProfile profile)
    {
        PowerPlanInfo? plan =
            PowerPlanService.FindProfile(profile);

        if (plan is null)
        {
            ShowError(
                $"Il profilo {profile} non è disponibile.");

            return;
        }

        uint result =
            PowerPlanService.SetActivePlan(plan.Guid);

        if (result != 0)
        {
            string message =
                new Win32Exception((int)result).Message;

            ShowError(
                $"Errore cambio profilo: {message}");

            return;
        }

        RefreshState();
    }

    private void ToggleStartup()
    {
        try
        {
            bool newState =
                !StartupService.IsEnabled();

            StartupService.SetEnabled(newState);

            _startupItem.Checked =
                StartupService.IsEnabled();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void RefreshState()
    {
        PowerPlanInfo? battery =
            PowerPlanService.FindProfile(
                PowerProfile.Battery);

        PowerPlanInfo? balanced =
            PowerPlanService.FindProfile(
                PowerProfile.Balanced);

        PowerPlanInfo? performance =
            PowerPlanService.FindProfile(
                PowerProfile.Performance);

        _batteryItem.Enabled =
            battery is not null;

        _balancedItem.Enabled =
            balanced is not null;

        _performanceItem.Enabled =
            performance is not null;

        _batteryItem.Checked = false;
        _balancedItem.Checked = false;
        _performanceItem.Checked = false;

        _startupItem.Checked =
            StartupService.IsEnabled();

        if (!PowerPlanService.TryGetActivePlan(
                out Guid activePlan,
                out _))
        {
            _trayIcon.Text =
                "Lenovo Control | Profilo sconosciuto";

            return;
        }

        if (battery?.Guid == activePlan)
            _batteryItem.Checked = true;

        if (balanced?.Guid == activePlan)
            _balancedItem.Checked = true;

        if (performance?.Guid == activePlan)
            _performanceItem.Checked = true;

        string name =
            PowerPlanService.GetPlanName(activePlan);

        string tooltip =
            $"Lenovo Control | {name}";

        _trayIcon.Text =
            tooltip.Length <= 63
                ? tooltip
                : tooltip[..63];
    }

    private void ShowError(string message)
    {
        _trayIcon.ShowBalloonTip(
            4000,
            "Lenovo Control",
            message,
            ToolTipIcon.Error);
    }

    protected override void ExitThreadCore()
    {
        _trayIcon.Visible = false;

        _trayIcon.Dispose();
        _applicationIcon.Dispose();
        _menu.Dispose();

        base.ExitThreadCore();
    }
}
