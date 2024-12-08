using System.Diagnostics;
using System.Configuration;
using System.Net;

public static class FileTransfer
{
     private const int _numOfSpaceChars = 15;  // スペース長さ
    public static async Task<string> TransferMeasurement(string sharePath, string logFilePath, int fileSizeMB)
    {
        byte[] data = new byte[fileSizeMB * 1024 * 1024];
        new Random().NextBytes(data);

        double ulTimeSecTotal = 0, ulTimeSecMax = 0, ulTimeSecMin = 0; // UpLoad(Read)
        double dnTimeSecTotal = 0, dnTimeSecMax = 0, dnTimeSecMin = 0; // Download(Write)

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
                var spc = new string(' ', _numOfSpaceChars);
                adr = adr + Environment.NewLine + spc + address.ToString();
            }
            idx++;
        }

        try
        {
            var overview = $"\n" +
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
                dnTimeSecTotal += sw;
                if (sw > dnTimeSecMax) dnTimeSecMax = sw;
                if (i == 1) dnTimeSecMin = sw;
                if (sw < dnTimeSecMin) dnTimeSecMin = sw;
            }
            
            double ulTimeSecAve = ulTimeSecTotal / loopCount; // アップロード平均時間sec
            double dnTimeSecAve = dnTimeSecTotal / loopCount; // ダウンロード平均時間sec

            double fileSizeBits = fileSizeMB * 8; // 単位をMBをBitに変換
            double ulSpeedMbpsAve = GetSpeed(fileSizeBits, ulTimeSecAve);
            double ulSpeedMbpsMax = GetSpeed(fileSizeBits, ulTimeSecMin); // 最速求めは最小の時間で割る[2]
            double ulSpeedMbpsMin = GetSpeed(fileSizeBits, ulTimeSecMax); // 最遅求めは最大の時間で割る[3]
            double dnSpeedMbpsAve = GetSpeed(fileSizeBits, dnTimeSecAve);
            double dnSpeedMbpsMax = GetSpeed(fileSizeBits, dnTimeSecMin); // [2]
            double dnSpeedMbpsMin = GetSpeed(fileSizeBits, dnTimeSecMax); // [3]

            var results = $"平均速度：" +
                        $"(↓) {dnSpeedMbpsAve.ToString("F1")} Mbps  {GetRoundCeiling(dnTimeSecAve, 3).ToString("F2")} Sec  " +
                        $"(↑) {ulSpeedMbpsAve.ToString("F1")} Mbps  {GetRoundCeiling(ulTimeSecAve, 3).ToString("F2")} Sec\n" +
                        $"最も速い：" +
                        $"(↓) {dnSpeedMbpsMax.ToString("F1")} Mbps  {GetRoundCeiling(dnTimeSecMin, 3).ToString("F2")} Sec  " +
                        $"(↑) {ulSpeedMbpsMax.ToString("F1")} Mbps  {GetRoundCeiling(ulTimeSecMin, 3).ToString("F2")} Sec\n" +
                        $"最も遅い：" +
                        $"(↓) {dnSpeedMbpsMin.ToString("F1")} Mbps  {GetRoundCeiling(dnTimeSecMax, 3).ToString("F2")} Sec  " +
                        $"(↑) {ulSpeedMbpsMin.ToString("F1")} Mbps  {GetRoundCeiling(ulTimeSecMax, 3).ToString("F2")} Sec\n" +
                        $"--------------------------------------------------\n";

            Console.Write($"{results}");
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

    // Mbpsを計算
    private static double GetSpeed(double fileSizeBits, double timeInSeconds)
    {
        return GetRoundFloor(fileSizeBits / timeInSeconds, 1);
    }

/// <summary>
/// 指定した小数点以下の桁数で切り捨て処理を行います
/// </summary>
/// <param name="value">対象の数値</param>
/// <param name="decimals">切り捨て処理を適用する小数点以下の桁数</param>
/// <returns>小数点以下が指定された桁数で切り捨てられた数値</returns>
/// <exception cref="ArgumentOutOfRangeException">
/// <paramref name="decimals"/> が 0 未満の場合にスローされます
/// </exception>
    public static double GetRoundFloor(double value, int decimals)
    {
        if (decimals < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(decimals), "Decimals cannot be negative.");
        }
        double factor = Math.Pow(10, decimals); // 10の指定された桁数倍
        return Math.Floor(value * factor) / factor; // 切り捨てて戻す
    }

/// <summary>
/// 指定した小数点以下の桁数で切り上げ処理を行います
/// </summary>
/// <param name="value">対象の数値</param>
/// <param name="decimals">切り上げ処理を適用する小数点以下の桁数</param>
/// <returns>小数点以下が指定された桁数で切り上げられた数値</returns>
/// <exception cref="ArgumentOutOfRangeException">
/// <paramref name="decimals"/> が 0 未満の場合にスローされます
/// </exception>
    public static double GetRoundCeiling(double value, int decimals)
    {
        if (decimals < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(decimals), "Decimals cannot be negative.");
        }

        double factor = Math.Pow(10, decimals);  // 10の指定された桁数倍
        return Math.Ceiling(value * factor) / factor;  // 切り上げて戻す
    }
}
