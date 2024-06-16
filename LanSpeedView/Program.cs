using System.Configuration;

namespace FileTransferApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // コマンドライン引数処理
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: LanSpeedView <sharePath> [-S <fileSizeMB>]");
                return;
            }

            string sharePath = args[0];
            // ファイルサイズデフォルト取得
            bool result = int.TryParse(ConfigurationManager.AppSettings["defaultFileSizeMB"], out int fileSizeMB);
            if (!result) return;

            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "-S" | args[i] == "-s" && i + 1 < args.Length && int.TryParse(args[i + 1], out int parsedSize))
                {
                    fileSizeMB = parsedSize;
                }
            }

            // 設定ファイルからログファイル名取得
            string logFilePath = ConfigurationManager.AppSettings["LogFilePath"] ?? "log.txt";

            // ファイル転送とログ記録の実行
            await FileTransferUtility.TransferAndLog(sharePath, logFilePath, fileSizeMB);

            // ユーザー応答待機
            // Console.WriteLine("Press any key to exit...");
            // Console.ReadKey();
        }
    }
}
