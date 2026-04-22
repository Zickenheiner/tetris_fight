namespace tetris_fight.Features.Board.Application;

using tetris_fight.Features.Board.Domain;

public class InputQueueService
{
    private readonly Queue<GameInput> _queue = new();

    public void Enqueue(GameInput input) => _queue.Enqueue(input);

    public GameInput[] DrainAll()
    {
        if (_queue.Count == 0) return [];
        var inputs = _queue.ToArray();
        _queue.Clear();
        return inputs;
    }
}
