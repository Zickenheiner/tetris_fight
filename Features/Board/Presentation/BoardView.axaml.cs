namespace tetris_fight.Features.Board.Presentation;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

public partial class BoardView : UserControl
{
    public BoardView()
    {
        InitializeComponent();
        DataContext = new BoardViewModel();
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
}
