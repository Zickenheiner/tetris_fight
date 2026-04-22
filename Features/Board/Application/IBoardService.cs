namespace tetris_fight.Features.Board.Application;

using tetris_fight.Features.Board.Domain;

public interface IBoardService
{
    BoardState State { get; }
    void SpawnPiece();
    bool TryMoveDown();
    bool TryMoveLeft();
    bool TryMoveRight();
    bool TryRotate(bool clockwise);
    void HardDrop();
    void LockPiece();
    int GetGhostRow();
    event Action? StateChanged;
}
