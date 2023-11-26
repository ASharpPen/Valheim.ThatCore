namespace ThatCore.Config.Toml.Writers;

public class TomlWriterSettings
{
    /// <summary>
    /// Text to add to top of file.
    /// </summary>
    public string Header { get; set; }

    public bool AddComments { get; set; }

    /// <summary>
    /// File name including extension to write toml to.
    /// </summary>
    public string FileName { get; set; }
}
