using Newtonsoft.Json;
using System.IO.Ports;
using System.Text;
using static Test.SIM800L.TerminalSIM800L.Medicion;

namespace Test.SIM800L
{
    /// <summary>
    /// Esta clase encapsula todo lo necesario para manejar un SIM800L. 
    /// Nota de aplicacion del fabricante:
    /// https://www.waveshare.com/w/upload/2/25/SIM800_Series_TCPIP_Application_Note_V1.03.pdf
    /// </summary>
    public partial class TerminalSIM800L : Form
    {
        public const string APN = "internet.movil";
        public const string APN_USER = "internet";
        public const string APN_PASSWORD = "internet";
        public const string TCP_Server_URL = "190.229.242.238";
        public const int TCP_Server_IP = 37000;

        public static ModuloSIM800L sim800L = new ModuloSIM800L();

        public TerminalSIM800L()
        {
            InitializeComponent();
        }

        private void TerminalSIM800L_Load(object sender, EventArgs e)
        {
            txtAPN.Text = APN;
            txtUsuario.Text = APN_USER;
            txtPassword.Text = APN_PASSWORD;
            txtServerURL.Text = TCP_Server_URL;
            numIP.Value = TCP_Server_IP;

            sim800L.ComandoEnviado += Sim800L_ComandoEnviado;
            sim800L.RespuestaRecibida += Sim800L_RespuestaRecibida;
            sim800L.OnEstadoActualizado += Sim800L_OnEstadoActualizado;
            ActualizarListaPuertos();
        }

        private void Sim800L_OnEstadoActualizado(string calidad)
        {
            if (lblCalidadSenial.InvokeRequired)
            {
                lblCalidadSenial.Invoke(() => Sim800L_OnEstadoActualizado(calidad));
                return;
            }

            lblCalidadSenial.Text = calidad;
        }

        private void Sim800L_RespuestaRecibida(string respuesta)
        {
            if (txtTerminal.InvokeRequired)
            {
                txtTerminal.Invoke(() => Sim800L_RespuestaRecibida(respuesta));
                return;
            }

            EscribirLinea(respuesta, request: false);
        }

        private void Sim800L_ComandoEnviado(string comando)
        {
            if (txtTerminal.InvokeRequired)
            {
                txtTerminal.Invoke(() => Sim800L_OnEstadoActualizado(comando));
                return;
            }

            EscribirLinea(comando, request: true);
        }

        private void EscribirLinea(string respuesta, bool request)
        {
            string tipoLinea = request ? "request" : "response";
            txtTerminal.Text += $"[{DateTime.Now}] [{tipoLinea}]: {respuesta}";
        }

        public class Medicion
        {
            public enum Unidad
            {
                GradoCelcius,
                Volt,
            }

            public Guid id;
            public DateTime fecha;
            public Unidad unidad;
            public int valor;

            public Medicion(int valor, Unidad unidad)
            {
                this.id = Guid.NewGuid();
                this.fecha = DateTime.Now;
                this.valor = valor;
                this.unidad = unidad;
            }
        }


        private void btnEjecutar_Click(object sender, EventArgs e)
        {
            try
            {

                var medicion = new Medicion(25, Unidad.Volt);

                var str = JsonConvert.SerializeObject(medicion);

                sim800L.EnviarPayload(Encoding.UTF8.GetBytes(str), timeoutMilisegundos: 30_000);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            try
            {
                sim800L.Restart();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnConectarServer_Click(object sender, EventArgs e)
        {
            try
            {
                sim800L.IniciarClienteTCP(txtServerURL.Text, (int)numIP.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnConectarAPN_Click(object sender, EventArgs e)
        {
            try
            {
                sim800L.ConectarAPN(txtAPN.Text, txtUsuario.Text, txtPassword.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnStatus_Click(object sender, EventArgs e)
        {
            try
            {
                sim800L.ActualizarEstado();

                lblCalidadSenial.Text = sim800L.CalidadSenial;
                lblConectado.Text = sim800L.Conectado.ToString();
                lblIP.Text = sim800L.IP;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void linkActualizar_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ActualizarListaPuertos();
        }

        private void ActualizarListaPuertos()
        {
            string[] puertos = SerialPort.GetPortNames();

            listaPuertos.Clear();
            foreach (var puerto in puertos)
            {
                listaPuertos.Items.Add(puerto);
            }
        }

        private void btnConectar_Click(object sender, EventArgs e)
        {
            if (listaPuertos.SelectedItems.Count == 0)
                return;

            var com = listaPuertos.SelectedItems[0].Text;
            try
            {
                sim800L.Iniciar(com);
                lblIP.Text = sim800L.IP;
                MessageBox.Show("Conectado");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnEnviar_Click(object sender, EventArgs e)
        {
            try
            {
                sim800L.EnviarComando(txtComando.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnCTRL_Z_Click(object sender, EventArgs e)
        {
            try
            {
                sim800L.EnviarCTRL_Z();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}