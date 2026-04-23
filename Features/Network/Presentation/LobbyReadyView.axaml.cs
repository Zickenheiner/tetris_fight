namespace tetris_fight.Features.Network.Presentation;

using Avalonia.Controls;
using Avalonia.Interactivity;
using tetris_fight.Features.Network.Infrastructure;

public partial class LobbyReadyView : UserControl
{
    public event Action<TcpNetworkService, bool>? GameReady;
    public event Action? ReturnToMenuRequested;

    public LobbyReadyView()
    {
        InitializeComponent();
    }

    public LobbyReadyView(TcpNetworkService network, string myPseudo, bool isHost)
        : this()
    {
        var vm = new LobbyReadyViewModel(network, myPseudo, isHost);
        vm.GameReady += (svc, host) => GameReady?.Invoke(svc, host);
        vm.ReturnToMenuRequested += () => ReturnToMenuRequested?.Invoke();
        DataContext = vm;
    }

    private void OnReadyClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is LobbyReadyViewModel vm)
            vm.SetReady();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is LobbyReadyViewModel vm)
            vm.Cancel();
    }

    protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (DataContext is LobbyReadyViewModel vm)
            vm.Dispose();
    }
}
