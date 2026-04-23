namespace tetris_fight.Features.Board.Domain;

public sealed record ForcedPieceAnimation(
    Tetromino? PreviousPiece,
    Point PreviousPosition,
    Tetromino IncomingPiece,
    Point IncomingPosition);
