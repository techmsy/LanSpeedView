using System.Diagnostics;
using System.Configuration;
using System.Net;

public static class FileTransferUtility
{
    public static async Task<string> TransferAndLog(string sharePath, string logFilePath, int fileSizeMB)
    {
        byte[] data = new byte[fileSizeMB * 1024 * 1024];
        new Random().NextBytes(data);

        Stopwatch stopwatch = new Stopwatch();

        double uploadTimeSeconds = 0, downloadTimeSeconds = 0, uploadSpeedMbps = 0, downloadSpeedMbps = 0;
        string remoteFilePath = Path.Combine(sharePath, Path.GetFileName(Path.GetTempFileName()));
        string currentPath = Directory.GetCurrentDirectory();

        bool result = int.TryParse(ConfigurationManager.AppSettings["loopCount"], out int iMax);
        if (!result) return "1";

        var hostname = Dns.GetHostName();
        IPAddress[] adrList = Dns.GetHostAddresses(hostname);

        var idx = 0;
        var adr = "";
        foreach (IPAddress address in adrList)
        {
            if (idx == 0)
            {
                adr = address.ToString();
            }
            else
            {
                var spc = "                     ";
                adr = adr + Environment.NewLine + spc + address.ToString();
            }
            idx++;
        }

        try
        {
            var overview = $"--------------------------------------------------\n" +
                           $"Date               : {DateTime.Now}\n" +
                           $"HostName           : {hostname}\n" +
                           $"IPAddress          : {adr}\n" +
                           $"File Size          : {fileSizeMB} MB\n" +
                           $"Loop Count         : {iMax} 回\n" +
                           $"Source Folder      : {currentPath}\n" +
                           $"Target Folder      : {sharePath}\n";
            Console.Write($"Process Start: {DateTime.Now}\n{overview}");

            // インジケーターの初期表示
            Console.Write("Progress: ");
            int progressLeft = Console.CursorLeft;

            for (var i = 1; i <= iMax; i++)
            {
                // インジケーターの更新
                try
                {
                    Console.SetCursorPosition(progressLeft, Console.CursorTop);
                    Console.Write(new string('.', i % 10)); // ドットを表示
                    Console.SetCursorPosition(progressLeft + 10, Console.CursorTop);
                    Console.Write($"{i}/{iMax}");
                }
                catch (IOException)
                {
                    // コンソールのカーソル位置設定に失敗した場合の処理
                    Console.WriteLine($"Progress: {i}/{iMax}");
                }

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

            var uploadTimeSecondsAve = Math.Round(uploadTimeSeconds / iMax, 2);
            var downloadTimeSecondsAve = Math.Round(downloadTimeSeconds / iMax, 2);
            var uploadSpeedMbpsAve = Math.Round(uploadSpeedMbps / iMax, 1);
            var downloadSpeedMbpsAve = Math.Round(downloadSpeedMbps / iMax, 1);

            var results = $"\n" +
                          $"Download(Read) Time: {downloadTimeSecondsAve} Sec\n" +
                          $"Upload(Write) Time : {uploadTimeSecondsAve} Sec\n" +
                          $"Download Speed     : {downloadSpeedMbpsAve} Mbps\n" +
                          $"Upload Speed       : {uploadSpeedMbpsAve} Mbps\n";

            Console.Write($"{results}");
            Console.WriteLine($"--------------------------------------------------");
            Console.WriteLine($"Process End: {DateTime.Now}\n");

            return overview + results;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return "1";
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
