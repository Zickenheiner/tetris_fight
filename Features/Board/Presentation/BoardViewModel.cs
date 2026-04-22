namespace tetris_fight.Features.Board.Presentation;

using Avalonia.Input;
using tetris_fight.Features.Board.Application;
using tetris_fight.Features.Board.Domain;
using tetris_fight.Features.Board.Infrastructure;

public class BoardViewModel
{
    private readonly IBoardService _boardService;
    private readonly GameLoopService _gameLoop;

    public CellViewModel[] Cells { get; }
    public CellViewModel[] NextPieceCells { get; }

    public BoardViewModel()
    {
        _boardService = new BoardService();
        _gameLoop = new GameLoopService(_boardService);

        Cells = Enumerable.Range(0, BoardState.Rows * BoardState.Cols)
                          .Select(_ => new CellViewModel())
                          .ToArray();

        NextPieceCells = Enumerable.Range(0, 4 * 4)
                                   .Select(_ => new CellViewModel())
                                   .ToArray();

        _boardService.StateChanged += RefreshGrid;
        _gameLoop.Start();
    }

    public void HandleKey(Key key)
    {
        switch (key)
        {
            case Key.Left:  _boardService.TryMoveLeft(); break;
            case Key.Right: _boardService.TryMoveRight(); break;
            case Key.Up:    _boardService.TryRotate(clockwise: true); break;
            case Key.Z:     _boardService.TryRotate(clockwise: false); break;
            case Key.Down:  _boardService.HardDrop(); break;
        }
    }

    private void RefreshGrid()
    {
        var state = _boardService.State;

        for (int r = 0; r < BoardState.Rows; r++)
            for (int c = 0; c < BoardState.Cols; c++)
                Cells[r * BoardState.Cols + c].Background = CellViewModel.GetBrush(state.Grid[r, c]);

        if (state.CurrentPiece is not null)
        {
            foreach (var cell in state.CurrentPiece.Cells)
            {
                int r = state.CurrentPosition.Row + cell.Row;
                int c = state.CurrentPosition.Col + cell.Col;
                if (r >= 0 && r < BoardState.Rows && c >= 0 && c < BoardState.Cols)
                    Cells[r * BoardState.Cols + c].Background = CellViewModel.GetBrush(state.CurrentPiece.Type);
            }
        }

        foreach (var cell in NextPieceCells)
            cell.Background = CellViewModel.GetBrush(null);

        if (state.NextPiece is not null)
        {
            foreach (var cell in state.NextPiece.Cells)
            {
                int idx = cell.Row * 4 + cell.Col;
                if (idx < NextPieceCells.Length)
                    NextPieceCells[idx].Background = CellViewModel.GetBrush(state.NextPiece.Type);
            }
        }
    }
}
