namespace tetris_fight.Features.Board.Domain;

public class Tetromino
{
    private static readonly Dictionary<TetrominoType, (Point[] Cells, int Size)> Shapes = new()
    {
        [TetrominoType.I] = ([new(1,0), new(1,1), new(1,2), new(1,3)], 4),
        [TetrominoType.O] = ([new(0,0), new(0,1), new(1,0), new(1,1)], 2),
        [TetrominoType.T] = ([new(0,1), new(1,0), new(1,1), new(1,2)], 3),
        [TetrominoType.S] = ([new(0,1), new(0,2), new(1,0), new(1,1)], 3),
        [TetrominoType.Z] = ([new(0,0), new(0,1), new(1,1), new(1,2)], 3),
        [TetrominoType.J] = ([new(0,0), new(1,0), new(1,1), new(1,2)], 3),
        [TetrominoType.L] = ([new(0,2), new(1,0), new(1,1), new(1,2)], 3),
    };

    public TetrominoType Type { get; }
    public Point[] Cells { get; }
    public int BoundingBoxSize { get; }

    public Tetromino(TetrominoType type)
    {
        Type = type;
        var (cells, size) = Shapes[type];
        Cells = cells.ToArray();
        BoundingBoxSize = size;
    }

    private Tetromino(TetrominoType type, Point[] cells, int size)
    {
        Type = type;
        Cells = cells;
        BoundingBoxSize = size;
    }

    public Tetromino RotateClockwise()
    {
        var rotated = Cells.Select(p => new Point(p.Col, BoundingBoxSize - 1 - p.Row)).ToArray();
        return new Tetromino(Type, rotated, BoundingBoxSize);
    }
}
