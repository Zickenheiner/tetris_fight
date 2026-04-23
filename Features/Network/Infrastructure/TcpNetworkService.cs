namespace tetris_fight.Features.Network.Infrastructure;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Avalonia.Threading;
using tetris_fight.Features.Board.Domain;
using tetris_fight.Features.Network.Application;
using tetris_fight.Features.Network.Domain;

public sealed class TcpNetworkService : INetworkService
{
    public const int Port = 55001;

    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private TcpListener? _listener;
    private TcpClient? _client;
    private NetworkStream? _stream;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private readonly object _writeLock = new();
    private bool _disposed;
    private int? _receivedSeed;

    public string LocalIpAddress { get; } = GetLocalIp();
    public int? ReceivedSeed => _receivedSeed;
    public int ListenPort => Port;

    public event Action? Connected;
    public event Action? Disconnected;
    public event Action<BoardSnapshot>? OpponentBoardReceived;
    public event Action<int>? PingUpdated;
    public event Action<int>? SeedReceived;
    public event Action<TetrominoType>? SabotageReceived;
    public event Action<string>? PlayerNameReceived;
    public event Action? ReadyReceived;
    public event Action? StartCountdownReceived;

    public async Task StartHostAsync(CancellationToken ct = default)
    {
        _listener = new TcpListener(IPAddress.Any, Port);
        _listener.Start();
        _client = await _listener.AcceptTcpClientAsync(ct);
        SetupStreams();
        Dispatcher.UIThread.Post(() => Connected?.Invoke());
        _ = ReceiveLoopAsync();
        StartPingTimer();
    }

    public async Task ConnectAsync(string ip, CancellationToken ct = default)
    {
        _client = new TcpClient();
        await _client.ConnectAsync(ip, Port, ct);
        SetupStreams();
        Dispatcher.UIThread.Post(() => Connected?.Invoke());
        _ = ReceiveLoopAsync();
        StartPingTimer();
    }

    public void SendBoard(BoardSnapshot snapshot)
    {
        var msg = new NetworkMessage
        {
            Type = NetworkMessageType.Board,
            Payload = JsonSerializer.Serialize(snapshot, JsonOpts)
        };
        SendMessage(msg);
    }

    public void SendSeed(int seed)
    {
        SendMessage(new NetworkMessage { Type = NetworkMessageType.Seed, Payload = seed.ToString() });
    }

    public void SendSabotage(TetrominoType type)
    {
        SendMessage(new NetworkMessage { Type = NetworkMessageType.Sabotage, Payload = ((int)type).ToString() });
    }

    public void SendPlayerName(string name)
    {
        SendMessage(new NetworkMessage { Type = NetworkMessageType.PlayerName, Payload = name });
    }

    public void SendReady()
    {
        SendMessage(new NetworkMessage { Type = NetworkMessageType.Ready });
    }

    public void SendStartCountdown()
    {
        SendMessage(new NetworkMessage { Type = NetworkMessageType.StartCountdown });
    }

    private void SetupStreams()
    {
        _stream = _client!.GetStream();
        _reader = new StreamReader(_stream, Encoding.UTF8);
        _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };
    }

    private void SendMessage(NetworkMessage msg)
    {
        if (_writer is null || _disposed) return;
        try
        {
            var json = JsonSerializer.Serialize(msg, JsonOpts) + "\n";
            lock (_writeLock)
                _writer.Write(json);
        }
        catch { /* connexion coupée */ }
    }

    private async Task ReceiveLoopAsync()
    {
        try
        {
            while (!_disposed && _reader is not null)
            {
                var line = await _reader.ReadLineAsync();
                if (line is null) break;

                var msg = JsonSerializer.Deserialize<NetworkMessage>(line, JsonOpts);
                if (msg is null) continue;

                HandleMessage(msg);
            }
        }
        catch { /* connexion coupée */ }
        finally
        {
            Dispatcher.UIThread.Post(() => Disconnected?.Invoke());
        }
    }

    private void HandleMessage(NetworkMessage msg)
    {
        switch (msg.Type)
        {
            case NetworkMessageType.Board when msg.Payload is not null:
                var snapshot = JsonSerializer.Deserialize<BoardSnapshot>(msg.Payload, JsonOpts);
                if (snapshot is not null)
                    Dispatcher.UIThread.Post(() => OpponentBoardReceived?.Invoke(snapshot));
                break;

            case NetworkMessageType.Ping:
                SendMessage(new NetworkMessage { Type = NetworkMessageType.Pong, Payload = msg.Payload });
                break;

            case NetworkMessageType.Pong when msg.Payload is not null:
                if (long.TryParse(msg.Payload, out long sent))
                {
                    int rtt = (int)(Environment.TickCount64 - sent);
                    Dispatcher.UIThread.Post(() => PingUpdated?.Invoke(rtt));
                }
                break;

            case NetworkMessageType.Seed when msg.Payload is not null:
                if (int.TryParse(msg.Payload, out int seed))
                {
                    _receivedSeed = seed;
                    Dispatcher.UIThread.Post(() => SeedReceived?.Invoke(seed));
                }
                break;

            case NetworkMessageType.Sabotage when msg.Payload is not null:
                if (int.TryParse(msg.Payload, out int typeId) && Enum.IsDefined(typeof(TetrominoType), typeId))
                    Dispatcher.UIThread.Post(() => SabotageReceived?.Invoke((TetrominoType)typeId));
                break;

            case NetworkMessageType.PlayerName when msg.Payload is not null:
                Dispatcher.UIThread.Post(() => PlayerNameReceived?.Invoke(msg.Payload));
                break;

            case NetworkMessageType.Ready:
                Dispatcher.UIThread.Post(() => ReadyReceived?.Invoke());
                break;

            case NetworkMessageType.StartCountdown:
                Dispatcher.UIThread.Post(() => StartCountdownReceived?.Invoke());
                break;
        }
    }

    private void StartPingTimer()
    {
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        timer.Tick += (_, _) =>
        {
            if (_disposed) { timer.Stop(); return; }
            SendMessage(new NetworkMessage
            {
                Type = NetworkMessageType.Ping,
                Payload = Environment.TickCount64.ToString()
            });
        };
        timer.Start();
    }

    private static string GetLocalIp()
    {
        try
        {
            using var s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            s.Connect("8.8.8.8", 65530);
            return ((IPEndPoint)s.LocalEndPoint!).Address.ToString();
        }
        catch { return "127.0.0.1"; }
    }

    public void Dispose()
    {
        _disposed = true;
        _client?.Close();
        _listener?.Stop();
        _reader?.Dispose();
        _writer?.Dispose();
    }
}
