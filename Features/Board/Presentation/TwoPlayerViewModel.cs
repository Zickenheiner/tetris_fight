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

    private bool _isPaused;
    public bool IsPaused
    {
        get => _isPaused;
        private set { _isPaused = value; OnPropertyChanged(); }
    }

    public event Action? ReturnToMenuRequested;

    public TwoPlayerViewModel()
    {
        PlayerBoard = new BoardViewModel(new BoardService(), stopMusicOnGameOver: false);
        PlayerBoard.ReturnToMenuRequested += () => ReturnToMenuRequested?.Invoke();
        PlayerBoard.PropertyChanged += OnBoardPropertyChanged;

        var aiService = new BoardService();
        AiBoard = new BoardViewModel(aiService, enableMusic: false);
        AiBoard.PropertyChanged += OnBoardPropertyChanged;
        _ai = new AiPlayerService(aiService);
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
