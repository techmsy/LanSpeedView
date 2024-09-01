using System;
using System.Diagnostics;
using System.Configuration;
using System.Net;
using System.IO;
using System.Threading.Tasks;

public static class FileTransferUtility
{
     private const int spaceChr = 21;  // スペース用空文字
    public static async Task<string> TransferAndLog(string sharePath, string logFilePath, int fileSizeMB)
    {
        byte[] data = new byte[fileSizeMB * 1024 * 1024];
        new Random().NextBytes(data);

        Stopwatch stopwatch = new Stopwatch();

        double uploadTimeSecTotal = 0, downloadTimeSecTotal = 0;
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
                var spc = new string(' ', spaceChr);
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

            for (var i = 1; i <= iMax; i++)
            {
                // インジケーターの更新
                Console.Write(".");
                if (i % 10 == 0 || i == iMax)
                {
                    Console.WriteLine($" {i}/{iMax} ({i * 100 / iMax}%)");
                }

                // Upload
                stopwatch.Restart();
                using (FileStream fs = new FileStream(remoteFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                {
                    await fs.WriteAsync(data, 0, data.Length);
                }
                stopwatch.Stop();
                uploadTimeSecTotal += stopwatch.Elapsed.TotalSeconds;

                // Download
                stopwatch.Restart();
                using (FileStream fs = new FileStream(remoteFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
                {
                    byte[] buffer = new byte[data.Length];
                    await fs.ReadAsync(buffer, 0, buffer.Length);
                }
                stopwatch.Stop();
                downloadTimeSecTotal += stopwatch.Elapsed.TotalSeconds;                
            }
            
            var uploadTimeSecondsAve = Math.Round(uploadTimeSecTotal / iMax, 2);
            var downloadTimeSecondsAve = Math.Round(downloadTimeSecTotal / iMax, 2);
            var uploadSpeedMbpsAve = Math.Round((fileSizeMB * 8) / uploadTimeSecondsAve, 1);
            var downloadSpeedMbpsAve = Math.Round((fileSizeMB * 8) / downloadTimeSecondsAve, 1);

            var results = $"\n" +
                          $"Download Time(Read) : {downloadTimeSecondsAve} Sec\n" +
                          $"Upload Time(Write)  : {uploadTimeSecondsAve} Sec\n" +
                          $"Download Speed(Read): {downloadSpeedMbpsAve} Mbps\n" +
                          $"Upload Speed(Write) : {uploadSpeedMbpsAve} Mbps\n";

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
