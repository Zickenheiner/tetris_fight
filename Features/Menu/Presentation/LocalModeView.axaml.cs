namespace tetris_fight.Features.Menu.Presentation;

using Avalonia.Controls;
using Avalonia.Interactivity;

public partial class LocalModeView : UserControl
{
    public event Action? SoloRequested;
    public event Action? VsAiRequested;
    public event Action? ReturnToMenuRequested;

    public LocalModeView()
    {
        InitializeComponent();
    }

    private void OnSoloClick(object? sender, RoutedEventArgs e) => SoloRequested?.Invoke();
    private void OnAiClick(object? sender, RoutedEventArgs e)   => VsAiRequested?.Invoke();
    private void OnBackClick(object? sender, RoutedEventArgs e) => ReturnToMenuRequested?.Invoke();
}
