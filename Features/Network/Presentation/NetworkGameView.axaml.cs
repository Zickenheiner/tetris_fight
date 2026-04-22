namespace tetris_fight.Features.Network.Presentation;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using tetris_fight.Features.Network.Infrastructure;

public partial class NetworkGameView : UserControl
{
    public event Action? ReturnToMenuRequested;

    public NetworkGameView()
    {
        InitializeComponent();
        Focusable = true;
    }

    public NetworkGameView(TcpNetworkService network, bool isHost)
        : this()
    {
        var vm = new NetworkGameViewModel(network, isHost);
        vm.ReturnToMenuRequested += () => ReturnToMenuRequested?.Invoke();
        DataContext = vm;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Focus();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (DataContext is NetworkGameViewModel vm)
        {
            vm.HandleKey(e.Key);
            e.Handled = true;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (DataContext is NetworkGameViewModel vm)
            vm.Dispose();
    }
}
