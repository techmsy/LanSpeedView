using System.Diagnostics;
using System.Configuration;

public static class FileTransferUtility
{
    public static async Task TransferAndLog(string sharePath, string logFilePath, int fileSizeMB)
    {
        byte[] data = new byte[fileSizeMB * 1024 * 1024];
        new Random().NextBytes(data);

        Stopwatch stopwatch = new Stopwatch();

        double uploadTimeSeconds = 0, downloadTimeSeconds = 0, uploadSpeedMbps = 0, downloadSpeedMbps = 0;
        string remoteFilePath = Path.Combine(sharePath, Path.GetFileName(Path.GetTempFileName()));
        string currentPath = Directory.GetCurrentDirectory();

        bool result = int.TryParse(ConfigurationManager.AppSettings["loopCount"], out int iMax);
        if (!result) return;

        try
        {
            var overview = $"--------------------------------------------------\n" +
                    $"Date               : {DateTime.Now}\n" +
                    $"File Size          : {fileSizeMB} MB\n" +
                    $"Source Folder      : {currentPath}\n" +
                    $"Target Folder      : {sharePath}\n";
            Console.Write($"Process Start: {DateTime.Now}\n{overview}");

            for (var i = 1; i <= iMax; i++) 
            {
            // Upload
            stopwatch.Start();
            await Task.Run(() => File.WriteAllBytes(remoteFilePath, data));
            stopwatch.Stop();
            uploadTimeSeconds += stopwatch.Elapsed.TotalSeconds;
            uploadSpeedMbps += (fileSizeMB * 8) / uploadTimeSeconds;

            // Download
            stopwatch.Restart();
            await Task.Run(() => File.ReadAllBytes(remoteFilePath));
            stopwatch.Stop();
            downloadTimeSeconds += stopwatch.Elapsed.TotalSeconds;
            downloadSpeedMbps += (fileSizeMB * 8) / downloadTimeSeconds;
            }

            var uploadTimeSecondsAve = Math.Round(uploadTimeSeconds / iMax,2);
            var downloadTimeSecondsAve = Math.Round(downloadTimeSeconds / iMax,2);
            var uploadSpeedMbpsAve = Math.Round(uploadSpeedMbps / iMax,1);
            var downloadSpeedMbpsAve = Math.Round(downloadSpeedMbps / iMax,1);

            var results = $"  {iMax} Times Average is \n" +
                        $"Download(Read) Time: {downloadTimeSecondsAve} Sec\n" +
                        $"Upload(Write) Time : {uploadTimeSecondsAve} Sec\n" +
                        $"Download Speed     : {downloadSpeedMbpsAve} Mbps\n" +
                        $"Upload Speed       : {uploadSpeedMbpsAve} Mbps\n";

            Logger.LogTransferResult(logFilePath, overview + results);

            // Console.WriteLine("Transfer complete. Check the log file for details.");
            Console.Write($"{results}");
            Console.WriteLine($"--------------------------------------------------");
            Console.WriteLine($"Process End: {DateTime.Now}\n");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        finally
        {
            if (File.Exists(remoteFilePath))
            {
                File.Delete(remoteFilePath);
            }
        }
    }
}
