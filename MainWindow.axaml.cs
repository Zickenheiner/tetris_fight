using Avalonia.Controls;
using tetris_fight.Features.Board.Presentation;
using tetris_fight.Features.Menu.Presentation;

namespace tetris_fight;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ShowMenu();
    }

    private void ShowMenu()
    {
        var vm = new MainMenuViewModel();
        vm.ItemActivated += HandleMenuAction;
        MainContent.Content = new MainMenuView { DataContext = vm };
    }

    private void HandleMenuAction(string action)
    {
        switch (action)
        {
            case "Jouer en local":
                var board = new BoardView();
                board.ReturnToMenuRequested += ShowMenu;
                MainContent.Content = board;
                break;
            case "Quitter":
                Close();
                break;
        }
    }
}
