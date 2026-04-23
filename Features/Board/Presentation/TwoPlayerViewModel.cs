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
    private readonly IBoardService _aiService;

    private bool _isPaused;
    public bool IsPaused
    {
        get => _isPaused;
        private set { _isPaused = value; OnPropertyChanged(); }
    }

    public event Action? ReturnToMenuRequested;

    public TwoPlayerViewModel()
    {
        _aiService = new BoardService();
        AiBoard = new BoardViewModel(_aiService);
        _ai = new AiPlayerService(_aiService);

        PlayerBoard = new BoardViewModel();
        PlayerBoard.ReturnToMenuRequested += () => ReturnToMenuRequested?.Invoke();
        PlayerBoard.SabotageActivated += type => _aiService.ForcePiece(type);
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

    public void Dispose() => _ai.Dispose();

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
