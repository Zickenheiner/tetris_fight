namespace tetris_fight.Features.Network.Application;

using tetris_fight.Features.Board.Domain;
using tetris_fight.Features.Network.Domain;

public interface INetworkService : IDisposable
{
    string LocalIpAddress { get; }
    int ListenPort { get; }

    Task StartHostAsync(CancellationToken ct = default);
    Task ConnectAsync(string ip, CancellationToken ct = default);
    void SendBoard(BoardSnapshot snapshot);
    void SendSeed(int seed);
    void SendSabotage(TetrominoType type);

    event Action? Connected;
    event Action? Disconnected;
    event Action<BoardSnapshot>? OpponentBoardReceived;
    event Action<int>? PingUpdated;
    event Action<int>? SeedReceived;
    event Action<TetrominoType>? SabotageReceived;
}
