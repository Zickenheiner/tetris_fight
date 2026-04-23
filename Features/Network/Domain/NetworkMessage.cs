namespace tetris_fight.Features.Network.Domain;

public enum NetworkMessageType { Board, Ping, Pong, Seed, Sabotage, PlayerName, Ready, StartCountdown }

public class NetworkMessage
{
    public NetworkMessageType Type { get; set; }
    public string? Payload { get; set; }
}
