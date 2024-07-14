using System.Configuration;

namespace FileTransferApp
{
    class Program
    {
        const string Title = "LanSpeedView";
        const string Version = "0.03";

        static async Task Main(string[] args)
        {
            var isLoged = false;

            // コマンドライン引数処理
            if (args.Length < 1)
            {
                Console.WriteLine($"{Title} {Version}");
                Console.WriteLine("Usage: LanSpeedView <sharePath> [-s <fileSizeMB>] [-savelog]");
                return;
            }

            string sharePath = args[0];
            // ファイルサイズデフォルト取得
            bool result = int.TryParse(ConfigurationManager.AppSettings["defaultFileSizeMB"], out int fileSizeMB);
            if (!result) return;

            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "-s" | args[i] == "-S" && i + 1 < args.Length && int.TryParse(args[i + 1], out int parsedSize))
                {
                    fileSizeMB = parsedSize;
                }
                if (args[i] == "-savelog")
                {
                    isLoged = true;
                }
            }

            // 設定ファイルからログファイル名取得
            string logFilePath = ConfigurationManager.AppSettings["LogFilePath"] ?? "log.txt";

            var logText = "";
            // ファイル転送の実行
            logText = await FileTransferUtility.TransferAndLog(sharePath, logFilePath, fileSizeMB);

            if (isLoged)
            {
                // ログ書き出し
                Logger.LogTransferResult(logFilePath, logText);
            }

            // ユーザー応答待機
            // Console.WriteLine("Press any key to exit...");
            // Console.ReadKey();
        }
    }
}
