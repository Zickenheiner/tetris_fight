namespace tetris_fight.Features.Board.Presentation;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media;
using tetris_fight.Features.Board.Domain;

public class CellViewModel : INotifyPropertyChanged
{
    private static readonly Dictionary<TetrominoType, IBrush> BrushMap = new()
    {
        [TetrominoType.I] = new SolidColorBrush(Color.Parse("#00FFFF")),
        [TetrominoType.O] = new SolidColorBrush(Color.Parse("#FFD700")),
        [TetrominoType.T] = new SolidColorBrush(Color.Parse("#AA00FF")),
        [TetrominoType.S] = new SolidColorBrush(Color.Parse("#00CC44")),
        [TetrominoType.Z] = new SolidColorBrush(Color.Parse("#FF3333")),
        [TetrominoType.J] = new SolidColorBrush(Color.Parse("#3366FF")),
        [TetrominoType.L] = new SolidColorBrush(Color.Parse("#FF8800")),
    };

    private static readonly Dictionary<TetrominoType, IBrush> GhostBrushMap = new()
    {
        [TetrominoType.I] = new SolidColorBrush(Color.FromArgb(60, 0, 255, 255)),
        [TetrominoType.O] = new SolidColorBrush(Color.FromArgb(60, 255, 215, 0)),
        [TetrominoType.T] = new SolidColorBrush(Color.FromArgb(60, 170, 0, 255)),
        [TetrominoType.S] = new SolidColorBrush(Color.FromArgb(60, 0, 204, 68)),
        [TetrominoType.Z] = new SolidColorBrush(Color.FromArgb(60, 255, 51, 51)),
        [TetrominoType.J] = new SolidColorBrush(Color.FromArgb(60, 51, 102, 255)),
        [TetrominoType.L] = new SolidColorBrush(Color.FromArgb(60, 255, 136, 0)),
    };

    private IBrush _background = Brushes.Transparent;

    public IBrush Background
    {
        get => _background;
        set
        {
            _background = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(CellBorderThickness));
        }
    }

    public bool IsEmpty => ReferenceEquals(_background, Brushes.Transparent);
    public Avalonia.Thickness CellBorderThickness => IsEmpty ? new Avalonia.Thickness(0) : new Avalonia.Thickness(1);

    public static IBrush GetBrush(TetrominoType? type) =>
        type.HasValue && BrushMap.TryGetValue(type.Value, out var b) ? b : Brushes.Transparent;

    public static IBrush GetGhostBrush(TetrominoType? type) =>
        type.HasValue && GhostBrushMap.TryGetValue(type.Value, out var b) ? b : Brushes.Transparent;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
