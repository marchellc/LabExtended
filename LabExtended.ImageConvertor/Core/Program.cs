using CommonLib;

namespace LabExtended.ImageConvertor;

/// <summary>
/// The entrypoint of this application.
/// </summary>
public static class Program
{
    /// <summary>
    /// Invoked when the application starts.
    /// </summary>
    /// <param name="args">The application arguments.</param>
    public static async Task Main(string[] args)
    {
        try
        {
            CommonLibrary.Initialize(args);
            
            Logger.Run();
            ImageConversion.Run();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
        }

        await Task.Delay(-1);
    }
}