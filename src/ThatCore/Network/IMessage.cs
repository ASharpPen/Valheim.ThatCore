namespace ThatCore.Network;

public interface IMessage
{
    void Initialize();

    void AfterUnpack();
}
