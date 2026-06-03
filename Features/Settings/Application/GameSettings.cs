namespace tetris_fight.Features.Settings.Application;

public sealed class GameSettings
{
    public double MusicVolume { get; set; } = 0.45;
    public bool ShowGhostPiece { get; set; } = true;
    public PlayerControlsSettings LocalPlayerControls { get; set; } = PlayerControlsSettings.Default();
}
