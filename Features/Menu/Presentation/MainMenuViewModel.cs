namespace tetris_fight.Features.Menu.Presentation;

using System.Collections.ObjectModel;

public class MainMenuViewModel
{
    public ObservableCollection<MenuItemViewModel> Items { get; }

    private int _selectedIndex;

    public event Action<string>? ItemActivated;

    public MainMenuViewModel()
    {
        Items = new ObservableCollection<MenuItemViewModel>
        {
            new("Jouer en local",        "#00F0F0"),           // cyan  — I
            new("Héberger une partie",   "#F0F000"),           // yellow — O
            new("Rejoindre une partie",  "#A000F0"),           // purple — T
            new("Paramètres",            "#F0A000"),           // orange — L
            new("Quitter",               "#F04040"),           // red   — Z
        };
        Items[0].IsSelected = true;
    }

    public void MoveUp()
    {
        Items[_selectedIndex].IsSelected = false;
        _selectedIndex = (_selectedIndex - 1 + Items.Count) % Items.Count;
        Items[_selectedIndex].IsSelected = true;
    }

    public void MoveDown()
    {
        Items[_selectedIndex].IsSelected = false;
        _selectedIndex = (_selectedIndex + 1) % Items.Count;
        Items[_selectedIndex].IsSelected = true;
    }

    public void Confirm()
    {
        var item = Items[_selectedIndex];
        if (!item.IsEnabled) return;
        ItemActivated?.Invoke(item.Label);
    }

    public void HoverItem(MenuItemViewModel item)
    {
        int idx = Items.IndexOf(item);
        if (idx < 0) return;
        Items[_selectedIndex].IsSelected = false;
        _selectedIndex = idx;
        Items[_selectedIndex].IsSelected = true;
    }

    public void ClickItem(MenuItemViewModel item)
    {
        HoverItem(item);
        Confirm();
    }
}
