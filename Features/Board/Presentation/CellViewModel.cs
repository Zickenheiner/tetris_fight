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

    private IBrush _background = Brushes.Transparent;

    public IBrush Background
    {
        get => _background;
        set { _background = value; OnPropertyChanged(); }
    }

    public static IBrush GetBrush(TetrominoType? type) =>
        type.HasValue && BrushMap.TryGetValue(type.Value, out var b) ? b : Brushes.Transparent;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
