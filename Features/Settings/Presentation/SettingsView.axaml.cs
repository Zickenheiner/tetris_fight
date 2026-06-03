namespace tetris_fight.Features.Settings.Presentation;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

public partial class SettingsView : UserControl
{
    public event Action? ReturnToMenuRequested;

    public SettingsView()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel();
        Focusable = true;
        AddHandler(KeyDownEvent, OnKeyDownTunnel, RoutingStrategies.Tunnel);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Focus();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (DataContext is SettingsViewModel { IsDuplicateKeyDialogVisible: true } dialogVm
            && e.Key is Key.Escape or Key.Enter)
        {
            dialogVm.CloseDuplicateKeyDialog();
            e.Handled = true;
            Focus();
            return;
        }

        if (DataContext is SettingsViewModel { CapturingAction: not null } vm)
        {
            vm.AssignCapturedKey(e.Key);
            e.Handled = true;
            Focus();
            return;
        }

        if (e.Key == Key.Escape)
        {
            ReturnToMenuRequested?.Invoke();
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    private void OnKeyDownTunnel(object? sender, KeyEventArgs e)
    {
        if (DataContext is SettingsViewModel { IsDuplicateKeyDialogVisible: true } dialogVm
            && e.Key is Key.Escape or Key.Enter)
        {
            dialogVm.CloseDuplicateKeyDialog();
            e.Handled = true;
            Focus();
            return;
        }

        if (DataContext is not SettingsViewModel { CapturingAction: not null } vm)
            return;

        vm.AssignCapturedKey(e.Key);
        e.Handled = true;
        Focus();
    }

    private void OnBackClick(object? sender, RoutedEventArgs e)
    {
        ReturnToMenuRequested?.Invoke();
    }
}
