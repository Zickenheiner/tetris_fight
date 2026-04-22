namespace tetris_fight.Features.Menu.Presentation;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media;

public class MenuItemViewModel : INotifyPropertyChanged
{
    public string Label { get; }
    public bool IsEnabled { get; }

    private readonly Color _accent;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ForegroundBrush));
            OnPropertyChanged(nameof(ItemFontWeight));
            OnPropertyChanged(nameof(BackgroundBrush));
            OnPropertyChanged(nameof(SelectionGlow));
        }
    }

    public IBrush AccentBrush => new SolidColorBrush(_accent);

    public IBrush ForegroundBrush => IsSelected
        ? Brushes.White
        : (IsEnabled
            ? new SolidColorBrush(Color.Parse("#8888aa"))
            : new SolidColorBrush(Color.Parse("#33334a")));

    public FontWeight ItemFontWeight => IsSelected ? FontWeight.Bold : FontWeight.Normal;

    public IBrush BackgroundBrush => IsSelected
        ? new SolidColorBrush(Color.FromArgb(28, _accent.R, _accent.G, _accent.B))
        : Brushes.Transparent;

    public double ItemOpacity => IsEnabled ? 1.0 : 0.45;

    public BoxShadows SelectionGlow
    {
        get
        {
            if (!IsSelected) return default;
            var outer = new BoxShadow
            {
                OffsetX = 0, OffsetY = 0,
                Blur = 22, Spread = 1,
                Color = Color.FromArgb(160, _accent.R, _accent.G, _accent.B)
            };
            var inner = new BoxShadow
            {
                OffsetX = 0, OffsetY = 0,
                Blur = 6, Spread = 0,
                Color = Color.FromArgb(80, _accent.R, _accent.G, _accent.B)
            };
            return new BoxShadows(outer, new[] { inner });
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public MenuItemViewModel(string label, string accentHex, bool isEnabled = true)
    {
        Label = label;
        IsEnabled = isEnabled;
        _accent = Color.Parse(accentHex);
    }
}
