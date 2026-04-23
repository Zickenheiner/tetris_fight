namespace tetris_fight.Features.Board.Presentation;

using System.ComponentModel;

public interface IBoardRenderViewModel : INotifyPropertyChanged
{
    CellViewModel[] Cells { get; }
    CellViewModel[] NextPieceCells { get; }
    int Score { get; }
    int LinesCleared { get; }
    bool IsGameOver { get; }
    double SabotageGaugePercent { get; }
    bool IsGaugeFull { get; }
}
