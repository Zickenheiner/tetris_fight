namespace tetris_fight.Features.Board.Presentation;

using tetris_fight.Features.Board.Application;
using tetris_fight.Features.Board.Infrastructure;

public sealed class TwoPlayerViewModel : IDisposable
{
    public BoardViewModel PlayerBoard { get; }
    public BoardViewModel AiBoard { get; }

    private readonly AiPlayerService _ai;

    public event Action? ReturnToMenuRequested;
    public void RequestReturnToMenu() => ReturnToMenuRequested?.Invoke();

    public TwoPlayerViewModel()
    {
        PlayerBoard = new BoardViewModel();
        PlayerBoard.ReturnToMenuRequested += () => ReturnToMenuRequested?.Invoke();

        var aiService = new BoardService();
        AiBoard = new BoardViewModel(aiService);
        _ai = new AiPlayerService(aiService);
    }

    public void Dispose() => _ai.Dispose();
}
