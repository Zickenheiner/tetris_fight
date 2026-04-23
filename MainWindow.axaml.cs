using Avalonia.Controls;
using tetris_fight.Features.Audio.Infrastructure;
using tetris_fight.Features.Board.Presentation;
using tetris_fight.Features.Menu.Presentation;
using tetris_fight.Features.Network.Infrastructure;
using tetris_fight.Features.Network.Presentation;

namespace tetris_fight;

public partial class MainWindow : Window
{
    private readonly GameMusicService _menuMusic = new(
        musicFile: "assets/sounds/accueil.mp3",
        baseRate: 1.0f,
        rateStep: 0f,
        maxRate: 1.0f);

    public MainWindow()
    {
        InitializeComponent();
        ShowMenu();
    }

    private void ShowMenu()
    {
        PlayMenuMusic();
        ResizeTo(460, 660);
        var vm = new MainMenuViewModel();
        vm.ItemActivated += HandleMenuAction;
        MainContent.Content = new MainMenuView { DataContext = vm };
    }

    private void HandleMenuAction(string action)
    {
        switch (action)
        {
            case "Jouer en local":
                ShowLocalModeSelection();
                break;
            case "Héberger une partie":
                ShowHostLobby();
                break;
            case "Rejoindre une partie":
                ShowJoin();
                break;
            case "Quitter":
                Close();
                break;
        }
    }

    private void ShowLocalModeSelection()
    {
        PlayMenuMusic();
        ResizeTo(460, 660);
        var view = new LocalModeView();
        view.SoloRequested        += ShowSoloGame;
        view.VsAiRequested        += ShowVsAiGame;
        view.ReturnToMenuRequested += ShowMenu;
        MainContent.Content = view;
    }

    private void ShowSoloGame()
    {
        StopMenuMusic();
        ResizeTo(460, 620);
        var view = new BoardView();
        view.ReturnToMenuRequested += ShowMenu;
        MainContent.Content = view;
    }

    private void ShowVsAiGame()
    {
        StopMenuMusic();
        ResizeTo(760, 620);
        var view = new TwoPlayerView();
        view.ReturnToMenuRequested += ShowMenu;
        MainContent.Content = view;
    }

    private void ShowHostLobby()
    {
        PlayMenuMusic();
        ResizeTo(460, 660);
        var view = new HostLobbyView();
        view.ReturnToMenuRequested += ShowMenu;
        view.GameReady += ShowLobbyReady;
        MainContent.Content = view;
    }

    private void ShowJoin()
    {
        PlayMenuMusic();
        ResizeTo(460, 660);
        var view = new JoinView();
        view.ReturnToMenuRequested += ShowMenu;
        view.GameReady += ShowLobbyReady;
        MainContent.Content = view;
    }

    private void ShowLobbyReady(TcpNetworkService network, string myPseudo, bool isHost)
    {
        ResizeTo(460, 660);
        var view = new LobbyReadyView(network, myPseudo, isHost);
        view.ReturnToMenuRequested += ShowMenu;
        view.GameReady += StartNetworkGame;
        MainContent.Content = view;
    }

    private void StartNetworkGame(TcpNetworkService network, bool isHost)
    {
        StopMenuMusic();
        ResizeTo(820, 620);
        var view = new NetworkGameView(network, isHost);
        view.ReturnToMenuRequested += ShowMenu;
        MainContent.Content = view;
    }

    private void PlayMenuMusic() => _menuMusic.Start();

    private void StopMenuMusic() => _menuMusic.Stop();

    private void ResizeTo(int width, int height)
    {
        Width = width;
        Height = height;
    }

    protected override void OnClosed(EventArgs e)
    {
        _menuMusic.Dispose();
        base.OnClosed(e);
    }
}
