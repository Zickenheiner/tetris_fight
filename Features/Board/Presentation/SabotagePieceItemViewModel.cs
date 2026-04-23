namespace tetris_fight.Features.Board.Presentation;

using System.Windows.Input;
using tetris_fight.Features.Board.Domain;
using tetris_fight.Features.Shared;

public sealed class SabotagePieceItemViewModel
{
    public TetrominoType Type { get; }
    public CellViewModel[] Cells { get; }
    public ICommand SelectCommand { get; }

    public SabotagePieceItemViewModel(TetrominoType type, Action<TetrominoType> onSelect)
    {
        Type = type;
        SelectCommand = new RelayCommand<object>(_ => onSelect(type));

        Cells = Enumerable.Range(0, 16).Select(_ => new CellViewModel()).ToArray();
        var piece = new Tetromino(type);
        foreach (var cell in piece.Cells)
        {
            int idx = cell.Row * 4 + cell.Col;
            if (idx < Cells.Length)
                Cells[idx].Background = CellViewModel.GetBrush(type);
        }
    }
}
