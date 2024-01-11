using System.IO.Ports;

namespace Test.SIM800L
{
    public partial class TerminalSIM800L : Form
    {
        public static string APN = "internet.movil";
        public static string APN_USER = "internet";
        public static string APN_PASSWORD = "internet";

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

            sim800L.ComandoEnviado += Sim800L_ComandoEnviado;
            sim800L.RespuestaRecibida += Sim800L_RespuestaRecibida;
            sim800L.OnEstadoActualizado += Sim800L_OnEstadoActualizado;
            ActualizarListaPuertos();
        }

        private void Sim800L_OnEstadoActualizado(string calidad)
        {
            if(lblCalidadSenial.InvokeRequired)
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
                sim800L.SetAPN(txtAPN.Text, txtUsuario.Text, txtPassword.Text);
                sim800L.RealizarRequestGET("http://www.brainjar.com/java/host/test.html", "brainjar.com", 80);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
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
                MessageBox.Show("Conectado");
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
    }
}