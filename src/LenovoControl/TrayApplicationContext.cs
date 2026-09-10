using System.ComponentModel;
using System.Drawing;

namespace LenovoControl;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly ContextMenuStrip _menu;

    private readonly ToolStripMenuItem _batteryItem;
    private readonly ToolStripMenuItem _balancedItem;
    private readonly ToolStripMenuItem _performanceItem;

    public TrayApplicationContext()
    {
        _batteryItem =
            new ToolStripMenuItem("Battery");

        _balancedItem =
            new ToolStripMenuItem("Balanced");

        _performanceItem =
            new ToolStripMenuItem("Performance");

        _batteryItem.Click += (_, _) =>
            ActivateProfile(PowerProfile.Battery);

        _balancedItem.Click += (_, _) =>
            ActivateProfile(PowerProfile.Balanced);

        _performanceItem.Click += (_, _) =>
            ActivateProfile(PowerProfile.Performance);

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

        _menu.Items.Add(exitItem);

        _menu.Opening += (_, _) =>
            RefreshState();

        _trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
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

        // NotifyIcon.Text ha un limite Windows.
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
        _menu.Dispose();

        base.ExitThreadCore();
    }
}
