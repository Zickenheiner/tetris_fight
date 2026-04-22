namespace tetris_fight.Features.Network.Domain;

public enum NetworkMessageType { Board, Ping, Pong }

public class NetworkMessage
{
    public NetworkMessageType Type { get; set; }
    public string? Payload { get; set; }
}
