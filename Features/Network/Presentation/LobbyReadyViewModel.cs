namespace tetris_fight.Features.Network.Presentation;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media;
using Avalonia.Threading;
using tetris_fight.Features.Network.Infrastructure;

public sealed class LobbyReadyViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly TcpNetworkService _network;
    private readonly bool _isHost;
    private bool _networkTransferred;
    private CancellationTokenSource? _countdownCts;

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
            OnPropertyChanged(nameof(CountdownBrush));
            OnPropertyChanged(nameof(CountdownGlowColor));
            OnPropertyChanged(nameof(IsShowingGo));
        }
    }

    public bool IsCountdownVisible => _countdown >= 0;
    public bool IsShowingGo => _countdown == 0;
    public string CountdownText => _countdown > 0 ? _countdown.ToString() : "GO!";

    public IBrush CountdownBrush => _countdown switch
    {
        3 => new SolidColorBrush(Color.Parse("#FF8040")),
        2 => new SolidColorBrush(Color.Parse("#FFD000")),
        1 => new SolidColorBrush(Color.Parse("#FF1850")),
        _ => new SolidColorBrush(Color.Parse("#00FF88"))
    };

    public string CountdownGlowColor => _countdown switch
    {
        3 => "#BBFF8040",
        2 => "#BBFFD000",
        1 => "#BBFF1850",
        _ => "#BB00FF88"
    };

    private double _countdownOpacity = 1.0;
    public double CountdownOpacity
    {
        get => _countdownOpacity;
        private set { _countdownOpacity = value; OnPropertyChanged(); }
    }

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

    private async void StartCountdown()
    {
        _countdownCts?.Cancel();
        _countdownCts = new CancellationTokenSource();
        var token = _countdownCts.Token;

        try
        {
            for (int i = 3; i >= 0; i--)
            {
                if (token.IsCancellationRequested) return;

                CountdownOpacity = 0.0;
                await Task.Delay(160, token);

                Countdown = i;
                CountdownOpacity = 1.0;

                int holdMs = i > 0 ? 840 : 1100;
                await Task.Delay(holdMs, token);
            }

            if (!token.IsCancellationRequested)
            {
                _networkTransferred = true;
                GameReady?.Invoke(_network, _isHost);
            }
        }
        catch (OperationCanceledException) { }
    }

    public void Cancel()
    {
        _countdownCts?.Cancel();
        ReturnToMenuRequested?.Invoke();
    }

    public void Dispose()
    {
        _countdownCts?.Cancel();
        _countdownCts?.Dispose();
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
