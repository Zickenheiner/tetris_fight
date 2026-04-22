namespace tetris_fight.Features.Board.Presentation;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

public partial class TwoPlayerView : UserControl
{
    public event Action? ReturnToMenuRequested;

    public TwoPlayerView()
    {
        InitializeComponent();
        var vm = new TwoPlayerViewModel();
        vm.ReturnToMenuRequested += () => ReturnToMenuRequested?.Invoke();
        DataContext = vm;
        Focusable = true;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Focus();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (DataContext is TwoPlayerViewModel vm)
        {
            if (e.Key == Key.Escape)
                vm.RequestReturnToMenu();
            else
                vm.PlayerBoard.HandleKey(e.Key);
            e.Handled = true;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (DataContext is TwoPlayerViewModel vm)
            vm.Dispose();
    }
}
