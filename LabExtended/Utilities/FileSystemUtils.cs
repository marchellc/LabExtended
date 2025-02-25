namespace LabExtended.Utilities;

public static class FileSystemUtils
{
    public static void CreateDirectory(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
            throw new ArgumentNullException(nameof(directoryPath));
        
        directoryPath = directoryPath.Replace('/', Path.DirectorySeparatorChar);
        
        var parts = directoryPath.Split(Path.DirectorySeparatorChar);
        var part = parts[0];
        
        for (int i = 0; i < parts.Length; i++)
        {
            if (!Directory.Exists(part))
                Directory.CreateDirectory(part);

            if (i + 1 >= parts.Length)
                break;
            
            part = Path.Combine(part, parts[i + 1]);
        }
    }
}