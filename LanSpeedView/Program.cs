using System.Configuration;

namespace FileTransferApp
{
    class Program
    {
        const string Title = "LanSpeedView";
        const string Version = "0.06";

        static async Task Main(string[] args)
        {
            var isRecordLog = false;

            // コマンドライン引数処理
            if (args.Length < 1)
            {
                Console.WriteLine($"{Title} {Version}");
                Console.WriteLine("Usage: LanSpeedView <sharePath> [-s <fileSizeMB>] [--savelog]");
                return;
            }

            // ファイルサイズデフォルト取得
            bool result = int.TryParse(ConfigurationManager.AppSettings["defaultFileSizeMB"], out int fileSizeMB);
            if (!result) return;

            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "-s" | args[i] == "-S" && i + 1 < args.Length && int.TryParse(args[i + 1], out int parsedSize))
                {
                    fileSizeMB = parsedSize;
                }
                if (args[i] == "--savelog")
                {
                    isRecordLog = true;
                }
            }

            // 設定ファイルからログファイル名取得
            string logFilePath = ConfigurationManager.AppSettings["logFilePath"] ?? "log.txt";

            // ファイル転送の実行
            string sharePath = args[0];
            var logText = await FileTransfer.TransferMeasurement(sharePath, logFilePath, fileSizeMB);

            if (isRecordLog)
            {
                Logger.LogTransferResult(logFilePath, logText); // ログ書き出し
            }
            // ユーザー応答待機
            // Console.WriteLine("Press any key to exit...");
            // Console.ReadKey();
        }
    }
}
