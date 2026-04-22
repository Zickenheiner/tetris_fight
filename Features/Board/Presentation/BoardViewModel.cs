namespace tetris_fight.Features.Board.Presentation;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using tetris_fight.Features.Board.Application;
using tetris_fight.Features.Board.Domain;
using tetris_fight.Features.Board.Infrastructure;

public class BoardViewModel : INotifyPropertyChanged, IBoardRenderViewModel
{
    private readonly IBoardService _boardService;
    private readonly GameLoopService _gameLoop;

    public CellViewModel[] Cells { get; }
    public CellViewModel[] NextPieceCells { get; }
    public bool ShowGhost { get; private set; } = true;

    private bool _isGameOver;
    public bool IsGameOver
    {
        get => _isGameOver;
        private set { _isGameOver = value; OnPropertyChanged(); }
    }

    private int _score;
    public int Score
    {
        get => _score;
        private set { _score = value; OnPropertyChanged(); }
    }

    private int _linesCleared;
    public int LinesCleared
    {
        get => _linesCleared;
        private set { _linesCleared = value; OnPropertyChanged(); }
    }

    public event Action? ReturnToMenuRequested;
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public BoardViewModel() : this(new BoardService()) { }

    public BoardViewModel(IBoardService boardService)
    {
        _boardService = boardService;
        _gameLoop = new GameLoopService(_boardService);

        Cells = Enumerable.Range(0, BoardState.Rows * BoardState.Cols)
                          .Select(_ => new CellViewModel())
                          .ToArray();

        NextPieceCells = Enumerable.Range(0, 4 * 4)
                                   .Select(_ => new CellViewModel())
                                   .ToArray();

        _boardService.StateChanged += RefreshGrid;
        _boardService.GameOver += StartGameOverAnimation;
        _gameLoop.Start();
    }

    private void StartGameOverAnimation()
    {
        int currentRow = BoardState.Rows - 1;
        var animBrush = new SolidColorBrush(Color.Parse("#882222"));
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(60) };
        timer.Tick += (_, _) =>
        {
            if (currentRow < 0)
            {
                timer.Stop();
                IsGameOver = true;
                return;
            }
            for (int c = 0; c < BoardState.Cols; c++)
                Cells[currentRow * BoardState.Cols + c].Background = animBrush;
            currentRow--;
        };
        timer.Start();
    }

    public void HandleKey(Key key)
    {
        if (IsGameOver)
        {
            if (key == Key.R) Restart();
            if (key == Key.Escape) ReturnToMenuRequested?.Invoke();
            return;
        }
        switch (key)
        {
            case Key.Left:  _boardService.TryMoveLeft(); break;
            case Key.Right: _boardService.TryMoveRight(); break;
            case Key.Up:    _boardService.TryRotate(clockwise: true); break;
            case Key.Z:     _boardService.TryRotate(clockwise: false); break;
            case Key.Down:  _boardService.TryMoveDown(); RefreshGrid(); break;
            case Key.Space: _boardService.HardDrop(); break;
            case Key.G:     ShowGhost = !ShowGhost; RefreshGrid(); break;
        }
    }

    private void Restart()
    {
        _gameLoop.Stop();
        IsGameOver = false;

        var state = _boardService.State;
        state.IsGameOver = false;
        state.CurrentPiece = null;
        state.NextPiece = null;
        state.Score = 0;
        state.LinesCleared = 0;
        Score = 0;
        LinesCleared = 0;
        for (int r = 0; r < BoardState.Rows; r++)
            for (int c = 0; c < BoardState.Cols; c++)
                state.Grid[r, c] = null;

        _gameLoop.Start();
    }

    private void RefreshGrid()
    {
        var state = _boardService.State;
        Score = state.Score;
        LinesCleared = state.LinesCleared;

        for (int r = 0; r < BoardState.Rows; r++)
            for (int c = 0; c < BoardState.Cols; c++)
                Cells[r * BoardState.Cols + c].Background = CellViewModel.GetBrush(state.Grid[r, c]);

        if (state.CurrentPiece is not null)
        {
            if (ShowGhost)
            {
                int ghostRow = _boardService.GetGhostRow();
                foreach (var cell in state.CurrentPiece.Cells)
                {
                    int r = ghostRow + cell.Row;
                    int c = state.CurrentPosition.Col + cell.Col;
                    if (r >= 0 && r < BoardState.Rows && c >= 0 && c < BoardState.Cols)
                        Cells[r * BoardState.Cols + c].Background = CellViewModel.GetGhostBrush(state.CurrentPiece.Type);
                }
            }

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
