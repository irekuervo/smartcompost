using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace MockSmartcompost.Utils
{
    public enum TipoDeLog
    {
        INFO,
        ERROR
    }

    public class FileLogger
    {
        public const string Extension = ".txt";
        public bool Habilitado { get; set; } = true;
        public bool PersistirDebug { get; set; } = true;
        public bool LogConsolaHabilitado { get; set; }
        public void PersistirLogs() => TimerAutoguardadoLog_Elapsed(null);
        public void IntegridadLogs() => TimerAutoborradoLog_Elapsed(null);

        const int minimumByteSizeFile = 1;
        const int defaultMaxByteSizeFile = 100;
        const int defaultMaxByteSizeAllFiles = 1000;
        const int defaultSegundosAutoguardado = 1;

        private int maxMbFileSize;
        private int maxMbAllFilesSize;
        private string nombreLog;
        private string rutaArchivoActual;
        private string directorio;
        private int segundosAutoguardado;
        private bool iniciarEnUltimoArchivoSiExiste;

        private Timer timerAutoguardadoLog;
        private Timer timerAutoBorradoLog;
        List<string> logsAcumulados = new List<string>();

        public FileLogger(
            string directorio,
            int segundosAutoguardado = defaultSegundosAutoguardado,
            int maxMbFileSize = defaultMaxByteSizeFile,
            bool iniciarEnUltimoArchivoSiExiste = true,
            string nombreLog = null,
            int maxMbAllFilesSize = defaultMaxByteSizeAllFiles,
            bool persistirAutomaticamente = true
            )
        {
            nombreLog = string.IsNullOrWhiteSpace(nombreLog) ? "log" : $"{nombreLog}-log";

            this.nombreLog = ObtenerNombreSeguro(nombreLog);

            this.iniciarEnUltimoArchivoSiExiste = iniciarEnUltimoArchivoSiExiste;

            // Directorio
            this.directorio = directorio;
            AsegurarDirectorio(directorio);

            // Minimo tamanio de archivos
            if (maxMbFileSize < minimumByteSizeFile)
                maxMbFileSize = minimumByteSizeFile;

            if (maxMbAllFilesSize < maxMbFileSize)
                maxMbAllFilesSize = maxMbFileSize;

            this.maxMbFileSize = maxMbFileSize;
            this.maxMbAllFilesSize = maxMbAllFilesSize;

            // Timer
            if (segundosAutoguardado < 1)
                segundosAutoguardado = 1;

            this.segundosAutoguardado = segundosAutoguardado;

            int milisegundosAutoguardado = this.segundosAutoguardado * 1000;

            if (persistirAutomaticamente)
                timerAutoguardadoLog = new Timer(TimerAutoguardadoLog_Elapsed, null, milisegundosAutoguardado, milisegundosAutoguardado);

            timerAutoBorradoLog = new Timer(TimerAutoborradoLog_Elapsed, null, milisegundosAutoguardado, milisegundosAutoguardado);
        }

        public string Log(string mensaje, string nombreMetodo = null) => LogBase(mensaje, TipoDeLog.INFO, nombreMetodo);

        public string LogError(Exception ex, string nombreMetodo = null) => LogBase(MensajeCompleto(ex), TipoDeLog.ERROR, nombreMetodo);

        public string LogError(string mensaje, string nombreMetodo = null) => LogBase(mensaje, TipoDeLog.ERROR, nombreMetodo);

        public string LogDebug(string mensaje, string nombreMetodo = null) => LogBase(mensaje, TipoDeLog.INFO, nombreMetodo);

        private string LogBase(string mensaje, TipoDeLog tipoLog, string nombreMetodo = null)
        {
            if (nombreMetodo == null)
                nombreMetodo = new StackTrace().GetFrame(2).GetMethod().Name;

            string log = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} | {tipoLog.ToString()} | {nombreMetodo} | {mensaje}";

            if (!Habilitado)
                return log;

            if (PersistirDebug == false && tipoLog == TipoDeLog.INFO)
                return log;

            if (LogConsolaHabilitado)
            {
                ConsoleColor colorAnterior = Console.ForegroundColor;
                Console.ForegroundColor = GetColor(tipoLog);
                Console.WriteLine(log);
                Console.ForegroundColor = colorAnterior;
            }

            lock (logsAcumulados)
            {
                logsAcumulados.Add(log);
            }

            return log;
        }

        #region Timers
        StringBuilder sb = new StringBuilder();
        private void TimerAutoguardadoLog_Elapsed(object sender)
        {
            lock (logsAcumulados)
            {
                try
                {
                    if (logsAcumulados.Count == 0)
                        return;

                    foreach (var log in logsAcumulados)
                    {
                        sb.Append(log + Environment.NewLine);
                    }

                    EscribirLogEnArchivo(sb.ToString());
                    sb.Clear();

                    logsAcumulados.Clear();
                }
                catch (Exception ex)
                {
                    string err = "Error autoguardado log: " + ex.Message;
                    Console.WriteLine(err);
                    Debug.Write(err);
                }
            }
        }

        bool borrando = false;
        private void TimerAutoborradoLog_Elapsed(object o)
        {
            if (borrando)
                return;

            try
            {
                borrando = true;

                List<string> logsViejos = ObtenerLogsViejos();

                // Borramos lo que se excede de los maximos
                if (logsViejos.Count > 1)
                {
                    long memoriaTotal = 0;
                    bool borrar = false;
                    for (int i = 1; i < logsViejos.Count; i++)
                    {
                        if (!borrar)
                        {
                            memoriaTotal += new FileInfo(logsViejos[i]).Length;
                            if (memoriaTotal > maxMbAllFilesSize * 1e6)
                                borrar = true;
                        }

                        if (borrar)
                        {
                            File.Delete(logsViejos[i]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string err = "Error autoborrado log: " + ex.Message;
                Console.WriteLine(err);
                Debug.Write(err);
            }
            finally
            {
                borrando = false;
            }
        }

        #endregion

        private void EscribirLogEnArchivo(string texto)
        {
            List<string> logsViejos = ObtenerLogsViejos();

            if (logsViejos.Count == 0 || this.iniciarEnUltimoArchivoSiExiste == false)
            {
                this.rutaArchivoActual = ObtenerNuevaRutaArchivo();

                File.AppendAllText(this.rutaArchivoActual, texto, Encoding.UTF8);

                return;
            }

            // Usamos el ultimo archivo
            this.rutaArchivoActual = logsViejos.First();

            // Si el ultimo es muy grande hacemos uno nuevo
            if (new FileInfo(this.rutaArchivoActual).Length >= this.maxMbFileSize * 1e6)
                this.rutaArchivoActual = ObtenerNuevaRutaArchivo();

            File.AppendAllText(this.rutaArchivoActual, texto, Encoding.UTF8);
        }

        private List<string> ObtenerLogsViejos()
        {
            if (Directory.Exists(directorio) == false)
                return new List<string>();

            var archivosTxt = Directory.GetFiles(this.directorio, $"*{Extension}");
            var logsViejos = archivosTxt.Where(c => Path.GetFileName(c).StartsWith(this.nombreLog)).OrderByDescending(c => c).ToList();
            return logsViejos;
        }

        private string ObtenerNuevaRutaArchivo()
        {
            if (Directory.Exists(directorio) == false)
                Directory.CreateDirectory(directorio);

            var rutaNueva = directorio + this.nombreLog + " " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + Extension;
            if (File.Exists(rutaNueva) == false)
            {
                var f = File.Create(rutaNueva);
                f.Close();
            }
            return rutaNueva;
        }

        private ConsoleColor GetColor(TipoDeLog tipoLog)
        {
            switch (tipoLog)
            {
                case TipoDeLog.INFO: return ConsoleColor.Yellow;
                case TipoDeLog.ERROR: return ConsoleColor.Red;
                default:
                    return ConsoleColor.White;
            }
        }

        public void Dispose()
        {
            timerAutoguardadoLog?.Dispose();

            /// Guardamos lo ultimo acumulado
            TimerAutoguardadoLog_Elapsed(null);

            timerAutoBorradoLog?.Dispose();

            /// Dejamos limpio todo
            TimerAutoborradoLog_Elapsed(null);

            try
            {
                // Si creamos un archivo y no se escribio nada lo borramos
                if (new FileInfo(this.rutaArchivoActual).Length == 0)
                    File.Delete(this.rutaArchivoActual);
            }
            catch (Exception)
            {
                // best effort
            }
        }

        private string AsegurarDirectorio(string directorio, bool silence = false)
        {
            try
            {
                if (!Directory.Exists(directorio))
                {
                    Directory.CreateDirectory(directorio);
                }

                return null;
            }
            catch (Exception ex)
            {
                return MensajeCompleto(ex);
            }
        }

        private string MensajeCompleto(Exception ex)
        {
            if (ex == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            do
            {
                sb.AppendLine(ex.Message);
                sb.AppendLine(ex.StackTrace);
                ex = ex.InnerException;
            }
            while (ex != null);

            return sb.ToString();
        }

        private string ObtenerNombreSeguro(string nombreDelArchivo)
        {
            if (string.IsNullOrEmpty(nombreDelArchivo)) return "archivo sin nombre";

            foreach (char character in Path.GetInvalidFileNameChars())
                nombreDelArchivo = nombreDelArchivo.Replace(character.ToString(), string.Empty);

            foreach (char character in Path.GetInvalidPathChars())
                nombreDelArchivo = nombreDelArchivo.Replace(character.ToString(), string.Empty);

            string nombre = Path.GetFileNameWithoutExtension(nombreDelArchivo);
            var reservedWords = new[] { "CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4",
                                        "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4",
                                        "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"};
            if (reservedWords.Contains(nombre.Trim().ToUpper()))
                nombreDelArchivo = "_" + nombreDelArchivo;

            return nombreDelArchivo;
        }
    }
}
