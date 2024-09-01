using System.Diagnostics;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;

public static class FileTransfer
{
     private const int spaceCount = 15;  // スペース長さ
    public static async Task<string> TransferMeasurement(string sharePath, string logFilePath, int fileSizeMB)
    {
        byte[] data = new byte[fileSizeMB * 1024 * 1024];
        new Random().NextBytes(data);

        double ulTimeSecTotal = 0, ulTimeSecMax = 0, ulTimeSecMin = 0; // UpLoad(Read)
        double dlTimeSecTotal = 0, dlTimeSecMax = 0, dlTimeSecMin = 0; // Download(Write)

        string remoteFilePath = Path.Combine(sharePath, Path.GetFileName(Path.GetTempFileName()));
        string currentPath = Directory.GetCurrentDirectory();

        // ループ回数取得
        bool result = int.TryParse(ConfigurationManager.AppSettings["loopCount"], out int loopCount);
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
                var spc = new string(' ', spaceCount);
                adr = adr + Environment.NewLine + spc + address.ToString();
            }
            idx++;
        }

        try
        {
            var overview = $"--------------------------------------------------\n" +
                            $"Date         : {DateTime.Now}\n" +
                            $"Host Name    : {hostname}\n" +
                            $"IP Address   : {adr}\n" +
                            $"File Size    : {fileSizeMB} MB\n" +
                            $"Loop Count   : {loopCount} 回\n" +
                            $"Source Folder: {currentPath}\n" +
                            $"Target Folder: {sharePath}\n";
            Console.Write($"Process Start: {DateTime.Now}\n{overview}");

            // インジケーターの初期表示
            Console.Write("Progress: ");

            Stopwatch stopwatch = new Stopwatch();
            for (var i = 1; i <= loopCount; i++)
            {
                // インジケーターの更新
                Console.Write(".");
                if (i % 10 == 0 || i == loopCount)
                {
                    Console.WriteLine($" {i}/{loopCount} ({i * 100 / loopCount}%)");
                }

                // アップロード時間計測
                stopwatch.Restart();
                using (FileStream fs = new FileStream(remoteFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                {
                    await fs.WriteAsync(data, 0, data.Length);
                }
                stopwatch.Stop();
                var sw = stopwatch.Elapsed.TotalSeconds;
                ulTimeSecTotal += sw;
                if (sw > ulTimeSecMax) ulTimeSecMax = sw;
                if (i == 1) ulTimeSecMin = sw;
                if (sw < ulTimeSecMin) ulTimeSecMin = sw;

                // ダウンロード時間計測
                stopwatch.Restart();
                using (FileStream fs = new FileStream(remoteFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
                {
                    byte[] buffer = new byte[data.Length];
                    await fs.ReadAsync(buffer, 0, buffer.Length);
                }
                stopwatch.Stop();
                sw = stopwatch.Elapsed.TotalSeconds;
                dlTimeSecTotal += sw;
                if (sw > dlTimeSecMax) dlTimeSecMax = sw;
                if (i == 1) dlTimeSecMin = sw;
                if (sw < dlTimeSecMin) dlTimeSecMin = sw;
            }
            
            double fileSizeBits = fileSizeMB * 8; // 定数計算のキャッシュ
            double ulTimeSecAve = GetRoundHalf(ulTimeSecTotal / loopCount, 2);
            double dlTimeSecAve = GetRoundHalf(dlTimeSecTotal / loopCount, 2);
            double ulSpeedMbpsAve = GetSpeed(fileSizeBits, ulTimeSecAve);
            double ulSpeedMbpsMax = GetSpeed(fileSizeBits, ulTimeSecMin); // 最速求めには最小の時間
            double ulSpeedMbpsMin = GetSpeed(fileSizeBits, ulTimeSecMax); // 最遅求めには最大の時間
            double dlSpeedMbpsAve = GetSpeed(fileSizeBits, dlTimeSecAve);
            double dlSpeedMbpsMax = GetSpeed(fileSizeBits, dlTimeSecMin); // 最速求めには最小の時間
            double dlSpeedMbpsMin = GetSpeed(fileSizeBits, dlTimeSecMax); // 最遅求めには最大の時間

            var results = $"\n" +
                            $"平均速度：\n" +
                            $"(↓) {dlSpeedMbpsAve} Mbps  {dlTimeSecAve} Sec\n" +
                            $"(↑) {ulSpeedMbpsAve} Mbps  {ulTimeSecAve} Sec\n" +
                            $"\n最も速い：\n" +
                            $"(↓) {dlSpeedMbpsMax} Mbps  {dlTimeSecMin} Sec\n" +
                            $"(↑) {ulSpeedMbpsMax} Mbps  {ulTimeSecMin} Sec\n" +
                            $"\n最も遅い：\n" +
                            $"(↓) {dlSpeedMbpsMin} Mbps  {dlTimeSecMax} Sec\n" +
                            $"(↑) {ulSpeedMbpsMin} Mbps  {ulTimeSecMax} Sec\n";

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
    // 四捨五入 (おそらく銀行丸め)
    public static double GetRoundHalf(double value, int decimals)
    {
        if (decimals < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(decimals), "Decimals cannot be negative.");
        }
        return Math.Round(value, decimals);
    }

    // Mbpsを計算
    private static double GetSpeed(double fileSizeBits, double timeInSeconds)
    {
        return GetRoundHalf(fileSizeBits / timeInSeconds, 1);
    }
}
