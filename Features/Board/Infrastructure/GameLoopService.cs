namespace tetris_fight.Features.Board.Infrastructure;

using Avalonia.Threading;
using tetris_fight.Features.Board.Application;

public class GameLoopService
{
    private readonly IBoardService _boardService;
    private readonly DispatcherTimer _timer;
    private int _tickCount;
    private int _speedLevel;

    public event Action<int>? SpeedLevelChanged;

    public GameLoopService(IBoardService boardService)
    {
        _boardService = boardService;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _timer.Tick += OnTick;
        _boardService.GameOver += Stop;
    }

    public void Start()
    {
        _tickCount = 0;
        _speedLevel = 0;
        _timer.Interval = TimeSpan.FromMilliseconds(500);
        SpeedLevelChanged?.Invoke(_speedLevel);
        _boardService.SpawnPiece();
        _timer.Start();
    }

    public void Stop() => _timer.Stop();
    public void Pause() => _timer.Stop();
    public void Resume() => _timer.Start();

    private void OnTick(object? sender, EventArgs e)
    {
        _tickCount++;

        // augmente la vitesse tous les 20 ticks (-15ms par palier, minimum 200ms)
        if (_tickCount % 20 == 0)
        {
            var faster = TimeSpan.FromMilliseconds(Math.Max(200, _timer.Interval.TotalMilliseconds - 15));
            _timer.Interval = faster;
            _speedLevel++;
            SpeedLevelChanged?.Invoke(_speedLevel);
        }

        if (!_boardService.TryMoveDown())
        {
            _boardService.LockPiece();
            _boardService.SpawnPiece();
        }
    }
}
