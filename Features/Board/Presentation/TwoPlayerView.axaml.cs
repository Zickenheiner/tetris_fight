namespace tetris_fight.Features.Board.Presentation;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

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
        if (DataContext is not TwoPlayerViewModel vm) return;

        if (e.Key == Key.Escape)
        {
            if (vm.IsMatchOver) ReturnToMenuRequested?.Invoke();
            else if (vm.IsPaused) vm.Resume();
            else vm.Pause();
        }
        else if (!vm.IsPaused && !vm.IsMatchOver)
        {
            vm.PlayerBoard.HandleKey(e.Key);
        }
        e.Handled = true;
    }

    private void OnResumeClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TwoPlayerViewModel vm)
            vm.Resume();
        Focus();
    }

    private void OnQuitClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TwoPlayerViewModel vm)
            vm.Resume();
        ReturnToMenuRequested?.Invoke();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (DataContext is TwoPlayerViewModel vm)
            vm.Dispose();
    }
}
