namespace tetris_fight.Features.Board.Presentation;

public interface IBoardRenderViewModel
{
    CellViewModel[] Cells { get; }
    CellViewModel[] NextPieceCells { get; }
    int Score { get; }
    int LinesCleared { get; }
    bool IsGameOver { get; }
}
