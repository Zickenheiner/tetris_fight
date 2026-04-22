namespace tetris_fight.Features.Board.Domain;

public class BoardState
{
    public const int Rows = 20;
    public const int Cols = 10;

    public TetrominoType?[,] Grid { get; } = new TetrominoType?[Rows, Cols];

    public Tetromino? CurrentPiece { get; set; }
    public Point CurrentPosition { get; set; }

    public Tetromino? NextPiece { get; set; }
}
