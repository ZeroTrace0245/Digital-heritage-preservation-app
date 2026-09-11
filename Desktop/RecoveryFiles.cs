using System;
using System.IO;
using System.Linq;
using System.Text;

namespace digital_heritage_preservation_app;

public static class RecoveryFiles
{
    public static string[] List(string path) => Directory.Exists(path + ".recovery")
        ? Directory.GetFiles(path + ".recovery", "*.json").OrderByDescending(f => Path.GetFileName(f), StringComparer.Ordinal).ToArray() : Array.Empty<string>();

    public static void Write(string path, string text)
    {
        path = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                var bytes = Encoding.UTF8.GetBytes(text);
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
            if (File.Exists(path))
            {
                Directory.CreateDirectory(path + ".recovery");
                var backup = Path.Combine(path + ".recovery", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff") + "-" + Guid.NewGuid().ToString("N") + ".json");
                File.Replace(temp, path, backup);
            }
            else File.Move(temp, path);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
        foreach (var old in List(path).Skip(10))
            try { File.Delete(old); } catch (IOException) { } catch (UnauthorizedAccessException) { }
    }
}
