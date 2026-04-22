namespace tetris_fight.Features.Board.Presentation;

using Avalonia.Controls;

public partial class BoardView : UserControl
{
    public BoardView()
    {
        InitializeComponent();
        DataContext = new BoardViewModel();
    }
}
