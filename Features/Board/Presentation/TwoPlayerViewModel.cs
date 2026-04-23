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

    private bool _playerDead;
    private bool _aiDead;
    private bool _matchEnded;
    private int _playerDeathScore;
    private int _aiDeathScore;

    private bool _isPaused;
    public bool IsPaused
    {
        get => _isPaused;
        private set { _isPaused = value; OnPropertyChanged(); }
    }

    private bool _isMatchOver;
    public bool IsMatchOver
    {
        get => _isMatchOver;
        private set { _isMatchOver = value; OnPropertyChanged(); }
    }

    private bool _isLocalPlayerWinner;
    public bool IsLocalPlayerWinner
    {
        get => _isLocalPlayerWinner;
        private set { _isLocalPlayerWinner = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsLocalPlayerLoser)); }
    }

    private bool _isMatchDraw;
    public bool IsMatchDraw
    {
        get => _isMatchDraw;
        private set { _isMatchDraw = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsLocalPlayerLoser)); }
    }

    public bool IsLocalPlayerLoser => !_isLocalPlayerWinner && !_isMatchDraw;

    public event Action? ReturnToMenuRequested;

    public TwoPlayerViewModel()
    {
        _playerService = new BoardService();
        _aiService = new BoardService();

        PlayerBoard = new BoardViewModel(_playerService, stopMusicOnGameOver: false);
        PlayerBoard.ReturnToMenuRequested += () => ReturnToMenuRequested?.Invoke();
        PlayerBoard.SabotageActivated += type => _aiService.ForcePiece(type);
        PlayerBoard.PropertyChanged += OnBoardPropertyChanged;

        AiBoard = new BoardViewModel(_aiService, enableMusic: false, showGhost: false, canSabotage: false);
        AiBoard.PropertyChanged += OnBoardPropertyChanged;
        _ai = new AiPlayerService(_aiService);

        _playerService.StateChanged += OnPlayerStateChanged;
        _aiService.StateChanged += OnAiStateChanged;
    }

    private void OnPlayerStateChanged()
    {
        int current = _playerService.State.LinesCleared;
        int delta = current - _playerPreviousLines;
        _playerPreviousLines = current;
        if (delta > 0) _aiService.AddSabotageCharge(delta);

        if (_aiDead && !_matchEnded && _playerService.State.Score > _aiDeathScore)
            EndMatch(1);
    }

    private void OnAiStateChanged()
    {
        int current = _aiService.State.LinesCleared;
        int delta = current - _aiPreviousLines;
        _aiPreviousLines = current;
        if (delta > 0) _playerService.AddSabotageCharge(delta);

        if (_playerDead && !_matchEnded && _aiService.State.Score > _playerDeathScore)
            EndMatch(-1);
    }

    private void OnBoardPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(BoardViewModel.IsGameOver)) return;

        if (sender == PlayerBoard && PlayerBoard.IsGameOver && !_playerDead)
            HandlePlayerDied();
        else if (sender == AiBoard && AiBoard.IsGameOver && !_aiDead)
            HandleAiDied();
    }

    private void HandlePlayerDied()
    {
        _playerDead = true;
        _playerDeathScore = PlayerBoard.Score;

        if (_aiDead)
        {
            EndMatch(_playerDeathScore.CompareTo(_aiDeathScore));
            return;
        }

        if (_playerDeathScore > AiBoard.Score)
        {
            // L'IA doit dépasser le score du joueur — suivi dans OnAiStateChanged
        }
        else
        {
            EndMatch(-1);
        }
    }

    private void HandleAiDied()
    {
        _aiDead = true;
        _aiDeathScore = AiBoard.Score;

        if (_playerDead)
        {
            EndMatch(_playerDeathScore.CompareTo(_aiDeathScore));
            return;
        }

        if (_aiDeathScore > PlayerBoard.Score)
        {
            // Le joueur doit dépasser le score de l'IA — suivi dans OnPlayerStateChanged
        }
        else
        {
            EndMatch(1);
        }
    }

    // sign > 0 : joueur gagne, sign < 0 : joueur perd, sign == 0 : égalité
    private void EndMatch(int sign)
    {
        if (_matchEnded) return;
        _matchEnded = true;

        _ai.Pause();
        if (!PlayerBoard.IsGameOver) PlayerBoard.Pause();
        if (!AiBoard.IsGameOver) AiBoard.Pause();
        PlayerBoard.StopMusic();

        if (sign == 0) IsMatchDraw = true;
        else IsLocalPlayerWinner = sign > 0;
        IsMatchOver = true;
    }

    public void Pause()
    {
        if (IsPaused || _matchEnded) return;
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
