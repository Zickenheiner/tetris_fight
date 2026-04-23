namespace tetris_fight.Features.Network.Domain;

public class BoardSnapshot
{
    /// <summary>
    /// Grille aplatie 200 entiers : 0 = vide, 1–7 = TetrominoType+1.
    /// Contient les cellules verrouillées ET la pièce courante rendue.
    /// </summary>
    public int[] Grid { get; set; } = new int[200];

    /// <summary>
    /// Pièce suivante aplatie sur 16 cases (4×4) : même encodage que Grid.
    /// </summary>
    public int[] NextPiece { get; set; } = new int[16];

    public int Score { get; set; }
    public int LinesCleared { get; set; }
    public bool IsGameOver { get; set; }
    public int SabotageCharge { get; set; }
}
