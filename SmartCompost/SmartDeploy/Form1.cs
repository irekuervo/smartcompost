using System.Diagnostics;
using System.IO.Ports;

namespace SmartDeploy
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ListAvailableComPorts();
        }

        private void ListAvailableComPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                comboBoxComPorts.Items.Add(port);
            }

            if (comboBoxComPorts.Items.Count > 0)
            {
                comboBoxComPorts.SelectedIndex = 0;
            }
        }

        private void buttonExecute_Click(object sender, EventArgs e)
        {
            if (comboBoxComPorts.SelectedItem != null)
            {
                string selectedPort = comboBoxComPorts.SelectedItem.ToString();

                //TODO: proceso:

                // 1) Tengo que pedir un numero de serie creando el nodo en la DB
                // 2) Si ya tengo un numero lo trato de reusar, quiza lo guarde en disco por si acaso
                // 3) tengo que seleccionar el tipo de nodo y compilar el proyecto
                // 4) creo el infoNodo.json y el deploy.json
                // 4) Quemo la salida del proyecto compilado al COM seleccionado

                ExecuteCommand(selectedPort);
            }
            else
            {
                MessageBox.Show("Please select a COM port first.");
            }
        }

        private void ExecuteCommand(string port)
        {
            try
            {
                // Ejemplo de comando que usa el puerto COM seleccionado
                string command = @$"nanoff --update --target ESP32_PSRAM_REV0 --serialport COM15  --masserase --fwversion 1.10.0.49  --filedeployment C:\tmp\deploy.json";
                
                ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardOutput = true;
                processInfo.RedirectStandardError = true;

                Process process = Process.Start(processInfo);
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                MessageBox.Show($"Output: {output}\nError: {error}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing command: {ex.Message}");
            }
        }
    }
}
