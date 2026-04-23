namespace tetris_fight.Features.Network.Presentation;

using Avalonia.Controls;
using Avalonia.Interactivity;
using tetris_fight.Features.Network.Infrastructure;

public partial class HostLobbyView : UserControl
{
    public event Action<TcpNetworkService, string, bool>? GameReady;
    public event Action? ReturnToMenuRequested;

    public HostLobbyView()
    {
        InitializeComponent();
        var vm = new HostLobbyViewModel();
        vm.GameReady += (svc, pseudo) => GameReady?.Invoke(svc, pseudo, true);
        vm.ReturnToMenuRequested += () => ReturnToMenuRequested?.Invoke();
        DataContext = vm;
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is HostLobbyViewModel vm)
            vm.Cancel();
    }

    protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (DataContext is HostLobbyViewModel vm)
            vm.Dispose();
    }
}
