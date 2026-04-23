namespace tetris_fight.Features.Board.Presentation;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

public partial class BoardView : UserControl
{
    public event Action? ReturnToMenuRequested;

    public BoardView()
    {
        InitializeComponent();
        var vm = new BoardViewModel();
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
        if (DataContext is BoardViewModel vm)
        {
            vm.HandleKey(e.Key);
            e.Handled = true;
        }
    }

    private void OnResumeClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is BoardViewModel vm)
            vm.Resume();
        Focus();
    }

    private void OnQuitClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is BoardViewModel vm)
            vm.Dispose();
        ReturnToMenuRequested?.Invoke();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (DataContext is BoardViewModel vm)
            vm.Dispose();
    }
}
