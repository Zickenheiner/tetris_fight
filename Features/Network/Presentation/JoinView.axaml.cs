namespace tetris_fight.Features.Network.Presentation;

using Avalonia.Controls;
using Avalonia.Interactivity;
using tetris_fight.Features.Network.Infrastructure;

public partial class JoinView : UserControl
{
    public event Action<TcpNetworkService, bool>? GameReady;
    public event Action? ReturnToMenuRequested;

    public JoinView()
    {
        InitializeComponent();
        var vm = new JoinViewModel();
        vm.GameReady += svc => GameReady?.Invoke(svc, false);
        vm.ReturnToMenuRequested += () => ReturnToMenuRequested?.Invoke();
        DataContext = vm;
    }

    private void OnConnectClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is JoinViewModel vm)
            vm.Connect();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is JoinViewModel vm)
            vm.Cancel();
    }

    protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (DataContext is JoinViewModel vm)
            vm.Dispose();
    }
}
