using System.IO;
using ThatCore.Logging;

namespace ThatCore.Utilities.Files;

public static class FileUtils
{
    public static void EnsureDirectoryExistsForFile(string filePath)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(dir))
        {
            Log.Trace?.Log("Creating missing folders in path.");
            Directory.CreateDirectory(dir);
        }
    }
}
