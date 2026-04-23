namespace tetris_fight.Features.Board.Presentation;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using tetris_fight.Features.Board.Domain;
using tetris_fight.Features.Network.Domain;

public class OpponentBoardViewModel : INotifyPropertyChanged, IBoardRenderViewModel
{
    public CellViewModel[] Cells { get; } =
        Enumerable.Range(0, BoardState.Rows * BoardState.Cols).Select(_ => new CellViewModel()).ToArray();

    public CellViewModel[] NextPieceCells { get; } =
        Enumerable.Range(0, 16).Select(_ => new CellViewModel()).ToArray();

    private int _score;
    public int Score { get => _score; private set { _score = value; OnPropertyChanged(); } }

    private int _linesCleared;
    public int LinesCleared { get => _linesCleared; private set { _linesCleared = value; OnPropertyChanged(); } }

    private bool _isGameOver;
    public bool IsGameOver { get => _isGameOver; private set { _isGameOver = value; OnPropertyChanged(); } }

    private double _sabotageGaugePercent;
    public double SabotageGaugePercent { get => _sabotageGaugePercent; private set { _sabotageGaugePercent = value; OnPropertyChanged(); } }

    private bool _isGaugeFull;
    public bool IsGaugeFull { get => _isGaugeFull; private set { _isGaugeFull = value; OnPropertyChanged(); } }

    public void UpdateFromSnapshot(BoardSnapshot snapshot)
    {
        Score = snapshot.Score;
        LinesCleared = snapshot.LinesCleared;
        IsGameOver = snapshot.IsGameOver;
        SabotageGaugePercent = snapshot.SabotageCharge / (double)BoardState.SabotageGaugeMax;
        IsGaugeFull = snapshot.SabotageCharge >= BoardState.SabotageGaugeMax;

        for (int i = 0; i < Math.Min(snapshot.Grid.Length, Cells.Length); i++)
        {
            var type = DecodeCell(snapshot.Grid[i]);
            Cells[i].Background = CellViewModel.GetBrush(type);
        }

        for (int i = 0; i < Math.Min(snapshot.NextPiece.Length, NextPieceCells.Length); i++)
        {
            var type = DecodeCell(snapshot.NextPiece[i]);
            NextPieceCells[i].Background = CellViewModel.GetBrush(type);
        }
    }

    private static TetrominoType? DecodeCell(int value) =>
        value == 0 ? null : (TetrominoType)(value - 1);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
