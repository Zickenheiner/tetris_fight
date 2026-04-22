namespace tetris_fight.Features.Network.Presentation;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using tetris_fight.Features.Network.Infrastructure;

public sealed class HostLobbyViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly TcpNetworkService _network = new();
    private readonly CancellationTokenSource _cts = new();
    private bool _networkTransferred;

    public string IpAddress => _network.LocalIpAddress;
    public int Port => _network.ListenPort;

    private string _status = "En attente d'un joueur...";
    public string Status
    {
        get => _status;
        private set { _status = value; OnPropertyChanged(); }
    }

    public event Action<TcpNetworkService>? GameReady;
    public event Action? ReturnToMenuRequested;

    public HostLobbyViewModel()
    {
        _network.Connected += OnConnected;
        _ = StartListeningAsync();
    }

    private async Task StartListeningAsync()
    {
        try
        {
            await _network.StartHostAsync(_cts.Token);
        }
        catch (OperationCanceledException) { }
        catch
        {
            Status = "Erreur lors de l'écoute réseau.";
        }
    }

    private void OnConnected()
    {
        Status = "Joueur connecté !";
        _networkTransferred = true;
        GameReady?.Invoke(_network);
    }

    public void Cancel()
    {
        _cts.Cancel();
        if (!_networkTransferred)
            _network.Dispose();
        ReturnToMenuRequested?.Invoke();
    }

    public void Dispose()
    {
        _network.Connected -= OnConnected;
        _cts.Cancel();
        _cts.Dispose();
        if (!_networkTransferred)
            _network.Dispose();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
