namespace MockSmartcompost.Utils
{
    public static class AppLogger
    {
        private static readonly FileLogger _logger;

        static AppLogger()
        {
            _logger = new FileLogger(@"C:\SmartCompost\");
        }

        public static void Log(string msg) => _logger.Log(msg);

    }
}
