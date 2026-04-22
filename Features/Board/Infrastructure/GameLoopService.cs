namespace tetris_fight.Features.Board.Infrastructure;

using Avalonia.Threading;
using tetris_fight.Features.Board.Application;

public class GameLoopService
{
    private readonly IBoardService _boardService;
    private readonly DispatcherTimer _timer;
    private int _tickCount;

    public GameLoopService(IBoardService boardService)
    {
        _boardService = boardService;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _timer.Tick += OnTick;
    }

    public void Start()
    {
        _tickCount = 0;
        _boardService.SpawnPiece();
        _timer.Start();
    }

    public void Stop() => _timer.Stop();

    private void OnTick(object? sender, EventArgs e)
    {
        _tickCount++;

        // augmente la vitesse tous les 20 ticks (-15ms par palier, minimum 200ms)
        if (_tickCount % 20 == 0)
        {
            var faster = TimeSpan.FromMilliseconds(Math.Max(200, _timer.Interval.TotalMilliseconds - 15));
            _timer.Interval = faster;
        }

        if (!_boardService.TryMoveDown())
        {
            _boardService.LockPiece();
            _boardService.SpawnPiece();
        }
    }
}
