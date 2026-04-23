namespace tetris_fight.Features.Board.Presentation;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using tetris_fight.Features.Board.Application;
using tetris_fight.Features.Board.Infrastructure;

public sealed class TwoPlayerViewModel : INotifyPropertyChanged, IDisposable
{
    public BoardViewModel PlayerBoard { get; }
    public BoardViewModel AiBoard { get; }

    private readonly AiPlayerService _ai;
    private readonly IBoardService _playerService;
    private readonly IBoardService _aiService;

    private int _playerPreviousLines;
    private int _aiPreviousLines;

    private bool _isPaused;
    public bool IsPaused
    {
        get => _isPaused;
        private set { _isPaused = value; OnPropertyChanged(); }
    }

    public event Action? ReturnToMenuRequested;

    public TwoPlayerViewModel()
    {
        _playerService = new BoardService();
        _aiService = new BoardService();

        PlayerBoard = new BoardViewModel(_playerService, stopMusicOnGameOver: false);
        PlayerBoard.ReturnToMenuRequested += () => ReturnToMenuRequested?.Invoke();
        PlayerBoard.SabotageActivated += type => _aiService.ForcePiece(type);
        PlayerBoard.PropertyChanged += OnBoardPropertyChanged;

        AiBoard = new BoardViewModel(_aiService, enableMusic: false, showGhost: false);
        AiBoard.PropertyChanged += OnBoardPropertyChanged;
        _ai = new AiPlayerService(_aiService);

        // Les lignes cassées par l'adversaire rechargent ta jauge
        _playerService.StateChanged += OnPlayerStateChanged;
        _aiService.StateChanged += OnAiStateChanged;
    }

    private void OnPlayerStateChanged()
    {
        int current = _playerService.State.LinesCleared;
        int delta = current - _playerPreviousLines;
        _playerPreviousLines = current;
        if (delta > 0) _aiService.AddSabotageCharge(delta);
    }

    private void OnAiStateChanged()
    {
        int current = _aiService.State.LinesCleared;
        int delta = current - _aiPreviousLines;
        _aiPreviousLines = current;
        if (delta > 0) _playerService.AddSabotageCharge(delta);
    }

    private void OnBoardPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BoardViewModel.IsGameOver))
            StopMusicWhenBothBoardsAreOver();
    }

    private void StopMusicWhenBothBoardsAreOver()
    {
        if (PlayerBoard.IsGameOver && AiBoard.IsGameOver)
            PlayerBoard.StopMusic();
    }

    public void Pause()
    {
        if (IsPaused) return;
        IsPaused = true;
        PlayerBoard.Pause();
        AiBoard.Pause();
        _ai.Pause();
    }

    public void Resume()
    {
        if (!IsPaused) return;
        IsPaused = false;
        PlayerBoard.Resume();
        AiBoard.Resume();
        _ai.Resume();
    }

    public void RequestReturnToMenu() => ReturnToMenuRequested?.Invoke();

    public void Dispose()
    {
        _playerService.StateChanged -= OnPlayerStateChanged;
        _aiService.StateChanged -= OnAiStateChanged;
        PlayerBoard.PropertyChanged -= OnBoardPropertyChanged;
        AiBoard.PropertyChanged -= OnBoardPropertyChanged;
        _ai.Dispose();
        PlayerBoard.Dispose();
        AiBoard.Dispose();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
