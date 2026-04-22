namespace tetris_fight.Features.Board.Infrastructure;

using Avalonia.Threading;
using tetris_fight.Features.Board.Application;
using tetris_fight.Features.Board.Domain;

public sealed class AiPlayerService : IAiPlayerService
{
    // Délai avant que l'IA agisse sur une nouvelle pièce (ms)
    private const int ThinkDelayMs = 800;

    // Probabilité (0–1) que l'IA choisisse une colonne aléatoire au lieu de l'optimale
    private const double MistakeRate = 0.30;

    private readonly IBoardService _board;
    private readonly Random _rng = new();
    private Tetromino? _lastPiece;
    private bool _disposed;

    public AiPlayerService(IBoardService board)
    {
        _board = board;
        _board.StateChanged += OnStateChanged;
    }

    private void OnStateChanged()
    {
        var state = _board.State;
        if (state.IsGameOver || state.CurrentPiece is null) return;
        if (ReferenceEquals(state.CurrentPiece, _lastPiece)) return;

        _lastPiece = state.CurrentPiece;

        // Délai de réflexion avant d'exécuter le mouvement
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ThinkDelayMs) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            ExecuteMove();
        };
        timer.Start();
    }

    private void ExecuteMove()
    {
        if (_disposed) return;
        var state = _board.State;
        if (state.IsGameOver || state.CurrentPiece is null) return;

        var (rotations, targetCol) = FindMove(state);

        for (int i = 0; i < rotations; i++)
            _board.TryRotate(clockwise: true);

        while (_board.State.CurrentPosition.Col > targetCol)
            if (!_board.TryMoveLeft()) break;

        while (_board.State.CurrentPosition.Col < targetCol)
            if (!_board.TryMoveRight()) break;

        // Pas de hard drop : la gravité naturelle gère la descente
    }

    private (int rotations, int col) FindMove(BoardState state)
    {
        // Erreur intentionnelle : colonne aléatoire valide
        if (_rng.NextDouble() < MistakeRate)
            return (0, _rng.Next(0, BoardState.Cols));

        return FindBestMove(state);
    }

    private static (int rotations, int col) FindBestMove(BoardState state)
    {
        double best = double.MinValue;
        int bestRot = 0, bestCol = 0;

        var piece = state.CurrentPiece!;

        for (int rot = 0; rot < 4; rot++)
        {
            for (int col = -2; col < BoardState.Cols + 2; col++)
            {
                var (valid, grid) = SimulateDrop(state.Grid, piece, col);
                if (!valid) continue;

                double score = Evaluate(grid);
                if (score > best)
                {
                    best = score;
                    bestRot = rot;
                    bestCol = col;
                }
            }
            piece = piece.RotateClockwise();
        }

        return (bestRot, bestCol);
    }

    private static (bool valid, TetrominoType?[,] grid) SimulateDrop(
        TetrominoType?[,] src, Tetromino piece, int startCol)
    {
        var grid = (TetrominoType?[,])src.Clone();
        var pos = new Point(0, startCol);

        if (!IsValid(piece, pos, grid)) return (false, grid);

        while (IsValid(piece, pos with { Row = pos.Row + 1 }, grid))
            pos = pos with { Row = pos.Row + 1 };

        foreach (var cell in piece.Cells)
        {
            int r = pos.Row + cell.Row;
            int c = pos.Col + cell.Col;
            if (r >= 0 && r < BoardState.Rows && c >= 0 && c < BoardState.Cols)
                grid[r, c] = piece.Type;
        }

        for (int r = BoardState.Rows - 1; r >= 0; r--)
        {
            if (!Enumerable.Range(0, BoardState.Cols).All(c => grid[r, c] is not null)) continue;
            for (int row = r; row > 0; row--)
                for (int c = 0; c < BoardState.Cols; c++)
                    grid[row, c] = grid[row - 1, c];
            for (int c = 0; c < BoardState.Cols; c++)
                grid[0, c] = null;
            r++;
        }

        return (true, grid);
    }

    private static bool IsValid(Tetromino piece, Point pos, TetrominoType?[,] grid)
    {
        foreach (var cell in piece.Cells)
        {
            int r = pos.Row + cell.Row;
            int c = pos.Col + cell.Col;
            if (r < 0 || r >= BoardState.Rows || c < 0 || c >= BoardState.Cols) return false;
            if (grid[r, c] is not null) return false;
        }
        return true;
    }

    private static double Evaluate(TetrominoType?[,] grid)
    {
        var heights = new int[BoardState.Cols];
        int holes = 0, lines = 0;

        for (int c = 0; c < BoardState.Cols; c++)
        {
            bool foundTop = false;
            for (int r = 0; r < BoardState.Rows; r++)
            {
                if (grid[r, c] is not null)
                {
                    if (!foundTop) { heights[c] = BoardState.Rows - r; foundTop = true; }
                }
                else if (foundTop) holes++;
            }
        }

        for (int r = 0; r < BoardState.Rows; r++)
            if (Enumerable.Range(0, BoardState.Cols).All(c => grid[r, c] is not null))
                lines++;

        int agg = heights.Sum();
        int bump = Enumerable.Range(0, BoardState.Cols - 1)
                             .Sum(c => Math.Abs(heights[c] - heights[c + 1]));

        return lines * 100.0 - agg * 0.51 - holes * 35.0 - bump * 6.0;
    }

    public void Dispose()
    {
        _disposed = true;
        _board.StateChanged -= OnStateChanged;
    }
}
