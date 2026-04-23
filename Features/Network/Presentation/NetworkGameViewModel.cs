namespace tetris_fight.Features.Network.Presentation;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Input;
using tetris_fight.Features.Board.Application;
using tetris_fight.Features.Board.Domain;
using tetris_fight.Features.Board.Infrastructure;
using tetris_fight.Features.Board.Presentation;
using tetris_fight.Features.Network.Application;
using tetris_fight.Features.Network.Domain;

public sealed class NetworkGameViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IBoardService _localService;
    private readonly INetworkService _network;

    public BoardViewModel LocalBoard { get; }
    public OpponentBoardViewModel OpponentBoard { get; } = new();

    private int _ping = -1;
    public int Ping { get => _ping; private set { _ping = value; OnPropertyChanged(); OnPropertyChanged(nameof(PingText)); } }
    public string PingText => _ping < 0 ? "— ms" : $"{_ping} ms";

    private string _connectionStatus = "Connecté";
    public string ConnectionStatus { get => _connectionStatus; private set { _connectionStatus = value; OnPropertyChanged(); } }

    private bool _isDisconnected;
    public bool IsDisconnected { get => _isDisconnected; private set { _isDisconnected = value; OnPropertyChanged(); } }

    private bool _isMenuOpen;
    public bool IsMenuOpen { get => _isMenuOpen; private set { _isMenuOpen = value; OnPropertyChanged(); } }

    public void OpenMenu() => IsMenuOpen = true;
    public void CloseMenu() => IsMenuOpen = false;

    public event Action? ReturnToMenuRequested;

    public NetworkGameViewModel(INetworkService network, bool isHost = false)
    {
        _network = network;
        _localService = new BoardService();
        _localService.StateChanged += OnLocalStateChanged;

        _network.OpponentBoardReceived += snap => OpponentBoard.UpdateFromSnapshot(snap);
        _network.PingUpdated += ms => Ping = ms;
        _network.Disconnected += OnDisconnected;
        _network.SeedReceived += OnSeedReceived;

        if (isHost)
        {
            // Seed appliquée avant le premier SpawnPiece
            int seed = Random.Shared.Next();
            _localService.SetSeed(seed);
            _network.SendSeed(seed);
            LocalBoard = new BoardViewModel(_localService, autoStart: true);
        }
        else
        {
            // Démarrage différé : on attend la seed avant de spawner la première pièce
            LocalBoard = new BoardViewModel(_localService, autoStart: false);
        }

        LocalBoard.ReturnToMenuRequested += () => ReturnToMenuRequested?.Invoke();
    }

    private void OnLocalStateChanged()
    {
        var state = _localService.State;
        var snapshot = BuildSnapshot(state);
        _network.SendBoard(snapshot);
    }

    private void OnSeedReceived(int seed)
    {
        _localService.SetSeed(seed);
        LocalBoard.Start();
    }

    private static BoardSnapshot BuildSnapshot(BoardState state)
    {
        var grid = new int[BoardState.Rows * BoardState.Cols];

        for (int r = 0; r < BoardState.Rows; r++)
            for (int c = 0; c < BoardState.Cols; c++)
                grid[r * BoardState.Cols + c] = EncodeCell(state.Grid[r, c]);

        if (state.CurrentPiece is not null)
        {
            foreach (var cell in state.CurrentPiece.Cells)
            {
                int r = state.CurrentPosition.Row + cell.Row;
                int cc = state.CurrentPosition.Col + cell.Col;
                if (r >= 0 && r < BoardState.Rows && cc >= 0 && cc < BoardState.Cols)
                    grid[r * BoardState.Cols + cc] = EncodeCell(state.CurrentPiece.Type);
            }
        }

        var next = new int[16];
        if (state.NextPiece is not null)
        {
            foreach (var cell in state.NextPiece.Cells)
            {
                int idx = cell.Row * 4 + cell.Col;
                if (idx < 16) next[idx] = EncodeCell(state.NextPiece.Type);
            }
        }

        return new BoardSnapshot
        {
            Grid = grid,
            NextPiece = next,
            Score = state.Score,
            LinesCleared = state.LinesCleared,
            IsGameOver = state.IsGameOver,
            SabotageCharge = state.SabotageCharge
        };
    }

    private static int EncodeCell(TetrominoType? type) =>
        type.HasValue ? (int)type.Value + 1 : 0;

    private void OnDisconnected()
    {
        ConnectionStatus = "Adversaire déconnecté";
        IsDisconnected = true;
    }

    public void HandleKey(Key key)
    {
        if (key == Key.Escape)
        {
            if (IsDisconnected) ReturnToMenuRequested?.Invoke();
            else if (IsMenuOpen) CloseMenu();
            else OpenMenu();
            return;
        }
        if (!IsMenuOpen)
            LocalBoard.HandleKey(key);
    }

    public void Dispose()
    {
        _localService.StateChanged -= OnLocalStateChanged;
        _network.SeedReceived -= OnSeedReceived;
        _network.Dispose();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
