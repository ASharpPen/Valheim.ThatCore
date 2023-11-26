namespace ThatCore.Network;

public class OutgoingMessage
{
    public ZPackage Package { get; set; }

    public ZRpc ZRpc { get; set; }

    public string Target { get; set; }

    public int Retries { get; set; }
}
