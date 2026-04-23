namespace tetris_fight.Features.Board.Presentation;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using tetris_fight.Features.Audio.Infrastructure;
using tetris_fight.Features.Board.Application;
using tetris_fight.Features.Board.Domain;
using tetris_fight.Features.Board.Infrastructure;

public class BoardViewModel : INotifyPropertyChanged, IBoardRenderViewModel, IDisposable
{
    private const int LineClearFrameMs = 40;
    private const int LineClearFrameCount = 6;
    private static readonly int[] LineClearDissolveOrder = { 1, 7, 3, 9, 0, 5, 2, 8, 4, 6 };
    private static readonly IBrush LineClearBrightBrush = new SolidColorBrush(Color.Parse("#F6FFB8"));
    private static readonly IBrush LineClearDimBrush = new SolidColorBrush(Color.Parse("#56F0F0"));

    private readonly IBoardService _boardService;
    private readonly GameLoopService _gameLoop;
    private readonly InputQueueService _inputQueue = new();
    private readonly DispatcherTimer _inputDrainTimer;
    private readonly GameMusicService? _music;
    private readonly bool _stopMusicOnGameOver;
    private bool _isLineClearAnimating;
    private int _lineClearAnimationVersion;
    private bool _disposed;

    public CellViewModel[] Cells { get; }
    public CellViewModel[] NextPieceCells { get; }
    public bool ShowGhost { get; private set; } = true;

    private bool _isGameOver;
    public bool IsGameOver
    {
        get => _isGameOver;
        private set { _isGameOver = value; OnPropertyChanged(); }
    }

    private bool _isPaused;
    public bool IsPaused
    {
        get => _isPaused;
        private set { _isPaused = value; OnPropertyChanged(); }
    }

    public void Pause()
    {
        if (_disposed || IsPaused) return;
        IsPaused = true;
        if (!IsGameOver)
            _gameLoop.Pause();
        _music?.Pause();
        _inputDrainTimer.Stop();
    }

    public void Resume()
    {
        if (_disposed || !IsPaused) return;
        IsPaused = false;
        _inputDrainTimer.Start();
        if (!IsGameOver)
            _gameLoop.Resume();
        _music?.Resume();
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

    public BoardViewModel(
        IBoardService boardService,
        bool autoStart = true,
        bool enableMusic = true,
        bool stopMusicOnGameOver = true)
    {
        _boardService = boardService;
        _gameLoop = new GameLoopService(_boardService);
        _music = enableMusic ? new GameMusicService() : null;
        _stopMusicOnGameOver = stopMusicOnGameOver;

        Cells = Enumerable.Range(0, BoardState.Rows * BoardState.Cols)
                          .Select(_ => new CellViewModel())
                          .ToArray();

        NextPieceCells = Enumerable.Range(0, 4 * 4)
                                   .Select(_ => new CellViewModel())
                                   .ToArray();

        _boardService.StateChanged += RefreshGrid;
        _boardService.GameOver += StartGameOverAnimation;
        _boardService.GameOver += StopInputDrain;
        if (_stopMusicOnGameOver)
            _boardService.GameOver += StopMusic;
        _gameLoop.SpeedLevelChanged += OnGameSpeedLevelChanged;

        _inputDrainTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _inputDrainTimer.Tick += DrainInputQueue;

        if (autoStart)
            Start();
    }

    public void Start()
    {
        if (_disposed)
            return;

        _music?.Start();
        _inputDrainTimer.Start();
        _gameLoop.Start();
    }

    private void DrainInputQueue(object? sender, EventArgs e)
    {
        foreach (var input in _inputQueue.DrainAll())
            ProcessInput(input);
    }

    private void ProcessInput(GameInput input)
    {
        switch (input)
        {
            case GameInput.MoveLeft:  _boardService.TryMoveLeft(); break;
            case GameInput.MoveRight: _boardService.TryMoveRight(); break;
            case GameInput.MoveDown:  _boardService.TryMoveDown(); break;
            case GameInput.RotateCW:  _boardService.TryRotate(clockwise: true); break;
            case GameInput.RotateCCW: _boardService.TryRotate(clockwise: false); break;
            case GameInput.HardDrop:  _boardService.HardDrop(); break;
        }
    }

    private void StopInputDrain() => _inputDrainTimer.Stop();
    public void StopMusic() => _music?.Stop();

    private void OnGameSpeedLevelChanged(int speedLevel) => _music?.SetSpeedLevel(speedLevel);

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
        if (key == Key.Escape)
        {
            if (IsPaused) Resume(); else Pause();
            return;
        }
        if (IsPaused) return;
        switch (key)
        {
            case Key.Left:  _inputQueue.Enqueue(GameInput.MoveLeft); break;
            case Key.Right: _inputQueue.Enqueue(GameInput.MoveRight); break;
            case Key.Up:    _inputQueue.Enqueue(GameInput.RotateCW); break;
            case Key.Z:     _inputQueue.Enqueue(GameInput.RotateCCW); break;
            case Key.Down:  _inputQueue.Enqueue(GameInput.MoveDown); break;
            case Key.Space: _inputQueue.Enqueue(GameInput.HardDrop); break;
            case Key.G:     ShowGhost = !ShowGhost; RefreshGrid(); break;
        }
    }

    private void Restart()
    {
        _gameLoop.Stop();
        _music?.Stop();
        _lineClearAnimationVersion++;
        _isLineClearAnimating = false;
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

        Start();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _gameLoop.Stop();
        _inputDrainTimer.Stop();
        _lineClearAnimationVersion++;
        _boardService.StateChanged -= RefreshGrid;
        _boardService.GameOver -= StartGameOverAnimation;
        _boardService.GameOver -= StopInputDrain;
        if (_stopMusicOnGameOver)
            _boardService.GameOver -= StopMusic;
        _gameLoop.SpeedLevelChanged -= OnGameSpeedLevelChanged;
        _music?.Dispose();
    }

    private void RefreshGrid()
    {
        var state = _boardService.State;
        int previousLinesCleared = LinesCleared;

        Score = state.Score;
        LinesCleared = state.LinesCleared;

        if (!_isLineClearAnimating
            && state.LinesCleared > previousLinesCleared
            && _boardService.LastClearedRows.Count > 0)
        {
            StartLineClearAnimation(_boardService.LastClearedRows);
            return;
        }

        if (_isLineClearAnimating)
            return;

        RenderGridFromState();
    }

    private void RenderGridFromState()
    {
        var state = _boardService.State;

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

    private void StartLineClearAnimation(IReadOnlyList<int> rows)
    {
        var rowsToAnimate = rows
            .Where(row => row >= 0 && row < BoardState.Rows)
            .Distinct()
            .ToArray();

        if (rowsToAnimate.Length == 0)
        {
            RenderGridFromState();
            return;
        }

        _isLineClearAnimating = true;
        int version = ++_lineClearAnimationVersion;
        int frame = 0;
        ApplyLineClearDissolveFrame(rowsToAnimate, frame);

        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(LineClearFrameMs) };
        timer.Tick += (_, _) =>
        {
            if (version != _lineClearAnimationVersion)
            {
                timer.Stop();
                return;
            }

            frame++;
            if (frame >= LineClearFrameCount)
            {
                timer.Stop();
                _isLineClearAnimating = false;
                RenderGridFromState();
                return;
            }

            ApplyLineClearDissolveFrame(rowsToAnimate, frame);
        };
        timer.Start();
    }

    private void ApplyLineClearDissolveFrame(int[] rows, int frame)
    {
        int transparentCells = frame * BoardState.Cols / (LineClearFrameCount - 1);
        var activeBrush = frame % 2 == 0 ? LineClearBrightBrush : LineClearDimBrush;

        foreach (int row in rows)
        {
            for (int orderIndex = 0; orderIndex < LineClearDissolveOrder.Length; orderIndex++)
            {
                int col = LineClearDissolveOrder[orderIndex];
                int cellIndex = row * BoardState.Cols + col;
                Cells[cellIndex].Background = orderIndex < transparentCells
                    ? Brushes.Transparent
                    : activeBrush;
            }
        }
    }
}
