namespace tetris_fight.Features.Network.Presentation;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using tetris_fight.Features.Network.Infrastructure;

public sealed class JoinViewModel : INotifyPropertyChanged, IDisposable
{
    private TcpNetworkService? _network;
    private readonly CancellationTokenSource _cts = new();
    private bool _networkTransferred;

    private string _ip = string.Empty;
    public string Ip
    {
        get => _ip;
        set
        {
            _ip = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanConnect));
        }
    }

    private string _pseudo = string.Empty;
    public string Pseudo
    {
        get => _pseudo;
        set { _pseudo = value; OnPropertyChanged(); }
    }

    public bool CanConnect => !string.IsNullOrWhiteSpace(_ip) && !_isConnecting;

    private bool _isConnecting;

    private string _status = string.Empty;
    public string Status
    {
        get => _status;
        private set { _status = value; OnPropertyChanged(); }
    }

    public event Action<TcpNetworkService, string>? GameReady;
    public event Action? ReturnToMenuRequested;

    public async void Connect()
    {
        if (!CanConnect) return;
        _isConnecting = true;
        OnPropertyChanged(nameof(CanConnect));
        Status = "Connexion en cours...";

        var network = new TcpNetworkService();
        _network = network;
        try
        {
            await network.ConnectAsync(_ip.Trim(), _cts.Token);
            Status = "Connecté !";
            _networkTransferred = true;
            _network = null;
            GameReady?.Invoke(network, string.IsNullOrWhiteSpace(_pseudo) ? "Joueur 2" : _pseudo.Trim());
        }
        catch (OperationCanceledException) { }
        catch
        {
            Status = "Connexion échouée. Vérifiez l'adresse IP.";
            _network?.Dispose();
            _network = null;
            _isConnecting = false;
            OnPropertyChanged(nameof(CanConnect));
        }
    }

    public void Cancel()
    {
        _cts.Cancel();
        ReturnToMenuRequested?.Invoke();
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        if (!_networkTransferred)
            _network?.Dispose();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
