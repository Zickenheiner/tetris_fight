namespace tetris_fight.Features.Board.Application;

using tetris_fight.Features.Board.Domain;

public class BoardService : IBoardService
{
    private readonly Random _random = new();

    public BoardState State { get; } = new();
    public event Action? StateChanged;

    public void SpawnPiece()
    {
        var piece = State.NextPiece ?? CreateRandom();
        State.NextPiece = CreateRandom();
        State.CurrentPiece = piece;
        State.CurrentPosition = new Point(0, (BoardState.Cols - piece.BoundingBoxSize) / 2);
        StateChanged?.Invoke();
    }

    public bool TryMoveDown()
    {
        if (State.CurrentPiece is null) return false;

        var next = State.CurrentPosition with { Row = State.CurrentPosition.Row + 1 };
        if (!IsValid(State.CurrentPiece, next)) return false;

        State.CurrentPosition = next;
        StateChanged?.Invoke();
        return true;
    }

    public bool TryMoveLeft()
    {
        if (State.CurrentPiece is null) return false;
        var next = State.CurrentPosition with { Col = State.CurrentPosition.Col - 1 };
        if (!IsValid(State.CurrentPiece, next)) return false;
        State.CurrentPosition = next;
        StateChanged?.Invoke();
        return true;
    }

    public bool TryMoveRight()
    {
        if (State.CurrentPiece is null) return false;
        var next = State.CurrentPosition with { Col = State.CurrentPosition.Col + 1 };
        if (!IsValid(State.CurrentPiece, next)) return false;
        State.CurrentPosition = next;
        StateChanged?.Invoke();
        return true;
    }

    public bool TryRotate(bool clockwise)
    {
        if (State.CurrentPiece is null) return false;
        var rotated = clockwise ? State.CurrentPiece.RotateClockwise() : State.CurrentPiece.RotateCounterClockwise();
        if (!IsValid(rotated, State.CurrentPosition)) return false;
        State.CurrentPiece = rotated;
        StateChanged?.Invoke();
        return true;
    }

    public void HardDrop()
    {
        if (State.CurrentPiece is null) return;
        while (true)
        {
            var next = State.CurrentPosition with { Row = State.CurrentPosition.Row + 1 };
            if (!IsValid(State.CurrentPiece, next)) break;
            State.CurrentPosition = next;
        }
        LockPiece();
    }

    public void LockPiece()
    {
        if (State.CurrentPiece is null) return;

        foreach (var cell in State.CurrentPiece.Cells)
        {
            int r = State.CurrentPosition.Row + cell.Row;
            int c = State.CurrentPosition.Col + cell.Col;
            if (r >= 0 && r < BoardState.Rows && c >= 0 && c < BoardState.Cols)
                State.Grid[r, c] = State.CurrentPiece.Type;
        }

        ClearFullLines();
        State.CurrentPiece = null;
        StateChanged?.Invoke();
    }

    private bool IsValid(Tetromino piece, Point pos)
    {
        foreach (var cell in piece.Cells)
        {
            int r = pos.Row + cell.Row;
            int c = pos.Col + cell.Col;
            if (r < 0 || r >= BoardState.Rows || c < 0 || c >= BoardState.Cols) return false;
            if (State.Grid[r, c] is not null) return false;
        }
        return true;
    }

    private void ClearFullLines()
    {
        for (int r = BoardState.Rows - 1; r >= 0; r--)
        {
            if (!Enumerable.Range(0, BoardState.Cols).All(c => State.Grid[r, c] is not null))
                continue;

            for (int row = r; row > 0; row--)
                for (int c = 0; c < BoardState.Cols; c++)
                    State.Grid[row, c] = State.Grid[row - 1, c];

            for (int c = 0; c < BoardState.Cols; c++)
                State.Grid[0, c] = null;

            r++;
        }
    }

    private Tetromino CreateRandom()
    {
        var types = Enum.GetValues<TetrominoType>();
        return new Tetromino(types[_random.Next(types.Length)]);
    }
}
