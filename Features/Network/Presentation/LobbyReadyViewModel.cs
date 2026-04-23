namespace tetris_fight.Features.Network.Presentation;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Threading;
using tetris_fight.Features.Network.Infrastructure;

public sealed class LobbyReadyViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly TcpNetworkService _network;
    private readonly bool _isHost;
    private bool _networkTransferred;

    public string MyPseudo { get; }

    private string _opponentPseudo = "...";
    public string OpponentPseudo
    {
        get => _opponentPseudo;
        private set { _opponentPseudo = value; OnPropertyChanged(); }
    }

    private bool _myReady;
    public bool MyReady
    {
        get => _myReady;
        private set { _myReady = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanClickReady)); }
    }

    private bool _opponentReady;
    public bool OpponentReady
    {
        get => _opponentReady;
        private set { _opponentReady = value; OnPropertyChanged(); }
    }

    public bool CanClickReady => !_myReady;

    private int _countdown = -1;
    public int Countdown
    {
        get => _countdown;
        private set
        {
            _countdown = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsCountdownVisible));
            OnPropertyChanged(nameof(CountdownText));
        }
    }

    public bool IsCountdownVisible => _countdown >= 0;
    public string CountdownText => _countdown > 0 ? _countdown.ToString() : "GO !";

    public event Action<TcpNetworkService, bool>? GameReady;
    public event Action? ReturnToMenuRequested;

    public LobbyReadyViewModel(TcpNetworkService network, string myPseudo, bool isHost)
    {
        _network = network;
        MyPseudo = myPseudo;
        _isHost = isHost;

        _network.PlayerNameReceived += OnOpponentNameReceived;
        _network.ReadyReceived += OnOpponentReady;
        _network.StartCountdownReceived += OnStartCountdown;

        _network.SendPlayerName(myPseudo);
    }

    private void OnOpponentNameReceived(string name) => OpponentPseudo = name;

    private void OnOpponentReady()
    {
        OpponentReady = true;
        if (_isHost && MyReady)
            TriggerCountdown();
    }

    public void SetReady()
    {
        if (!CanClickReady) return;
        MyReady = true;
        _network.SendReady();
        if (_isHost && OpponentReady)
            TriggerCountdown();
    }

    private void OnStartCountdown()
    {
        if (!_isHost)
            StartCountdown();
    }

    private void TriggerCountdown()
    {
        _network.SendStartCountdown();
        StartCountdown();
    }

    private void StartCountdown()
    {
        Countdown = 3;
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (_, _) =>
        {
            if (Countdown > 0)
            {
                Countdown--;
            }
            else
            {
                timer.Stop();
                _networkTransferred = true;
                GameReady?.Invoke(_network, _isHost);
            }
        };
        timer.Start();
    }

    public void Cancel()
    {
        ReturnToMenuRequested?.Invoke();
    }

    public void Dispose()
    {
        _network.PlayerNameReceived -= OnOpponentNameReceived;
        _network.ReadyReceived -= OnOpponentReady;
        _network.StartCountdownReceived -= OnStartCountdown;
        if (!_networkTransferred)
            _network.Dispose();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
