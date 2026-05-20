namespace tetris_fight.Features.Settings.Application;

using Avalonia.Input;
using tetris_fight.Features.Board.Domain;

public sealed class PlayerControlsSettings
{
    public Key MoveLeft { get; set; } = Key.Left;
    public Key MoveRight { get; set; } = Key.Right;
    public Key MoveDown { get; set; } = Key.Down;
    public Key RotateClockwise { get; set; } = Key.Up;
    public Key RotateCounterClockwise { get; set; } = Key.Z;
    public Key HardDrop { get; set; } = Key.Space;

    public GameInput? ToGameInput(Key key) =>
        key == MoveLeft ? GameInput.MoveLeft :
        key == MoveRight ? GameInput.MoveRight :
        key == MoveDown ? GameInput.MoveDown :
        key == RotateClockwise ? GameInput.RotateCW :
        key == RotateCounterClockwise ? GameInput.RotateCCW :
        key == HardDrop ? GameInput.HardDrop :
        null;

    public Key GetKey(ControlAction action) => action switch
    {
        ControlAction.MoveLeft => MoveLeft,
        ControlAction.MoveRight => MoveRight,
        ControlAction.MoveDown => MoveDown,
        ControlAction.RotateClockwise => RotateClockwise,
        ControlAction.RotateCounterClockwise => RotateCounterClockwise,
        ControlAction.HardDrop => HardDrop,
        _ => Key.None
    };

    public void SetKey(ControlAction action, Key key)
    {
        switch (action)
        {
            case ControlAction.MoveLeft: MoveLeft = key; break;
            case ControlAction.MoveRight: MoveRight = key; break;
            case ControlAction.MoveDown: MoveDown = key; break;
            case ControlAction.RotateClockwise: RotateClockwise = key; break;
            case ControlAction.RotateCounterClockwise: RotateCounterClockwise = key; break;
            case ControlAction.HardDrop: HardDrop = key; break;
        }
    }

    public ControlAction? FindActionForKey(Key key, ControlAction exceptAction)
    {
        foreach (var action in Enum.GetValues<ControlAction>())
        {
            if (action != exceptAction && GetKey(action) == key)
                return action;
        }

        return null;
    }

    public static PlayerControlsSettings Default() => new();
}
