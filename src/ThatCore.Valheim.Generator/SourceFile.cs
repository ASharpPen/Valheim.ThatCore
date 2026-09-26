namespace ThatCore.Valheim.Generator;

internal ref struct SourceFile(
    string hintName, 
    string source)
{
    public string HintName = hintName;
    public string Source = source;
}
