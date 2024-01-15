using System.IO.Ports;
using System.Text;

namespace Test.SIM800L
{
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


            txtTerminal.Text += "\r\n" + respuesta;
        }

        private void Sim800L_ComandoEnviado(string comando)
        {
            if (txtTerminal.InvokeRequired)
            {
                txtTerminal.Invoke(() => Sim800L_OnEstadoActualizado(comando));
                return;
            }

            txtTerminal.Text += comando;
        }

        private void btnEjecutar_Click(object sender, EventArgs e)
        {
            try
            {
                sim800L.EnviarPayload(Encoding.UTF8.GetBytes("hola carola!"), timeoutMilisegundos: 30_000);
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
    }
}