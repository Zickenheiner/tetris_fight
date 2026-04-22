namespace tetris_fight.Features.Board.Application;

using tetris_fight.Features.Board.Domain;

public interface IBoardService
{
    BoardState State { get; }
    void SpawnPiece();
    bool TryMoveDown();
    void LockPiece();
    event Action? StateChanged;
}
