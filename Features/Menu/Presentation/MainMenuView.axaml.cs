namespace tetris_fight.Features.Menu.Presentation;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

public partial class MainMenuView : UserControl
{
    public MainMenuView()
    {
        InitializeComponent();
        Focusable = true;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Focus();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (DataContext is MainMenuViewModel vm)
        {
            switch (e.Key)
            {
                case Key.Up:    vm.MoveUp();   e.Handled = true; return;
                case Key.Down:  vm.MoveDown(); e.Handled = true; return;
                case Key.Enter: vm.Confirm();  e.Handled = true; return;
            }
        }
        base.OnKeyDown(e);
    }

    private void OnItemPointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is Control { Tag: MenuItemViewModel item } && DataContext is MainMenuViewModel vm)
            vm.HoverItem(item);
    }

    private void OnItemPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Control { Tag: MenuItemViewModel item } && DataContext is MainMenuViewModel vm)
            vm.ClickItem(item);
    }
}
