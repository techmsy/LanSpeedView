public static class Logger
{
    public static void LogTransferResult(string logFilePath, string results)
    {
        File.AppendAllText(logFilePath, results);
    }
}
