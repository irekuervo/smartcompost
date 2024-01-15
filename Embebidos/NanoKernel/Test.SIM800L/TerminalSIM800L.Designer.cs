namespace Test.SIM800L
{
    partial class TerminalSIM800L
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnEjecutar = new Button();
            txtTerminal = new RichTextBox();
            txtAPN = new TextBox();
            txtUsuario = new TextBox();
            txtPassword = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            btnConectar = new Button();
            listaPuertos = new ListView();
            puerto = new ColumnHeader();
            linkActualizar = new LinkLabel();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            lblCalidadSenial = new Label();
            lblConectado = new Label();
            lblIP = new Label();
            label8 = new Label();
            txtComando = new TextBox();
            btnEnviar = new Button();
            btnConectarAPN = new Button();
            label9 = new Label();
            txtServerURL = new TextBox();
            btnConectarServer = new Button();
            numIP = new NumericUpDown();
            btnRestart = new Button();
            btnStatus = new Button();
            ((System.ComponentModel.ISupportInitialize)numIP).BeginInit();
            SuspendLayout();
            // 
            // btnEjecutar
            // 
            btnEjecutar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnEjecutar.BackColor = SystemColors.ControlDark;
            btnEjecutar.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point);
            btnEjecutar.ForeColor = Color.Black;
            btnEjecutar.Location = new Point(12, 543);
            btnEjecutar.Name = "btnEjecutar";
            btnEjecutar.Size = new Size(271, 40);
            btnEjecutar.TabIndex = 0;
            btnEjecutar.Text = "Enviar Test TCP";
            btnEjecutar.UseVisualStyleBackColor = false;
            btnEjecutar.Click += btnEjecutar_Click;
            // 
            // txtTerminal
            // 
            txtTerminal.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtTerminal.Location = new Point(289, 46);
            txtTerminal.Name = "txtTerminal";
            txtTerminal.Size = new Size(561, 657);
            txtTerminal.TabIndex = 1;
            txtTerminal.Text = "";
            // 
            // txtAPN
            // 
            txtAPN.Location = new Point(91, 287);
            txtAPN.Name = "txtAPN";
            txtAPN.Size = new Size(192, 23);
            txtAPN.TabIndex = 2;
            // 
            // txtUsuario
            // 
            txtUsuario.Location = new Point(91, 321);
            txtUsuario.Name = "txtUsuario";
            txtUsuario.Size = new Size(192, 23);
            txtUsuario.TabIndex = 3;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(91, 354);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(192, 23);
            txtPassword.TabIndex = 4;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label1.ForeColor = Color.FromArgb(64, 64, 64);
            label1.Location = new Point(12, 295);
            label1.Name = "label1";
            label1.Size = new Size(34, 15);
            label1.TabIndex = 5;
            label1.Text = "APN:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label2.ForeColor = Color.FromArgb(64, 64, 64);
            label2.Location = new Point(12, 329);
            label2.Name = "label2";
            label2.Size = new Size(52, 15);
            label2.TabIndex = 6;
            label2.Text = "Usuario:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label3.ForeColor = Color.FromArgb(64, 64, 64);
            label3.Location = new Point(12, 362);
            label3.Name = "label3";
            label3.Size = new Size(62, 15);
            label3.TabIndex = 7;
            label3.Text = "Password:";
            // 
            // btnConectar
            // 
            btnConectar.Location = new Point(12, 155);
            btnConectar.Name = "btnConectar";
            btnConectar.Size = new Size(75, 23);
            btnConectar.TabIndex = 8;
            btnConectar.Text = "Conectar";
            btnConectar.UseVisualStyleBackColor = true;
            btnConectar.Click += btnConectar_Click;
            // 
            // listaPuertos
            // 
            listaPuertos.Columns.AddRange(new ColumnHeader[] { puerto });
            listaPuertos.FullRowSelect = true;
            listaPuertos.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listaPuertos.HoverSelection = true;
            listaPuertos.Location = new Point(12, 46);
            listaPuertos.MultiSelect = false;
            listaPuertos.Name = "listaPuertos";
            listaPuertos.Size = new Size(271, 103);
            listaPuertos.TabIndex = 9;
            listaPuertos.UseCompatibleStateImageBehavior = false;
            listaPuertos.View = View.SmallIcon;
            // 
            // puerto
            // 
            puerto.Text = "Puerto";
            // 
            // linkActualizar
            // 
            linkActualizar.AutoSize = true;
            linkActualizar.Location = new Point(12, 25);
            linkActualizar.Name = "linkActualizar";
            linkActualizar.Size = new Size(59, 15);
            linkActualizar.TabIndex = 10;
            linkActualizar.TabStop = true;
            linkActualizar.Text = "Actualizar";
            linkActualizar.LinkClicked += linkActualizar_LinkClicked;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 201);
            label4.Name = "label4";
            label4.Size = new Size(81, 15);
            label4.TabIndex = 11;
            label4.Text = "Calidad Señal:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(12, 227);
            label5.Name = "label5";
            label5.Size = new Size(97, 15);
            label5.TabIndex = 12;
            label5.Text = "Conectado a red:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(12, 255);
            label6.Name = "label6";
            label6.Size = new Size(73, 15);
            label6.TabIndex = 13;
            label6.Text = "Dirección IP:";
            // 
            // lblCalidadSenial
            // 
            lblCalidadSenial.Location = new Point(127, 201);
            lblCalidadSenial.Name = "lblCalidadSenial";
            lblCalidadSenial.Size = new Size(156, 22);
            lblCalidadSenial.TabIndex = 14;
            lblCalidadSenial.Text = "-";
            // 
            // lblConectado
            // 
            lblConectado.Location = new Point(127, 227);
            lblConectado.Name = "lblConectado";
            lblConectado.Size = new Size(156, 22);
            lblConectado.TabIndex = 15;
            lblConectado.Text = "-";
            // 
            // lblIP
            // 
            lblIP.Location = new Point(127, 255);
            lblIP.Name = "lblIP";
            lblIP.Size = new Size(156, 22);
            lblIP.TabIndex = 16;
            lblIP.Text = "-";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label8.ForeColor = Color.FromArgb(64, 64, 64);
            label8.Location = new Point(289, 25);
            label8.Name = "label8";
            label8.Size = new Size(62, 15);
            label8.TabIndex = 17;
            label8.Text = "Comando:";
            // 
            // txtComando
            // 
            txtComando.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtComando.Location = new Point(357, 17);
            txtComando.Name = "txtComando";
            txtComando.Size = new Size(370, 23);
            txtComando.TabIndex = 18;
            // 
            // btnEnviar
            // 
            btnEnviar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnEnviar.BackColor = SystemColors.ControlDark;
            btnEnviar.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point);
            btnEnviar.ForeColor = Color.Black;
            btnEnviar.Location = new Point(733, 11);
            btnEnviar.Name = "btnEnviar";
            btnEnviar.Size = new Size(117, 29);
            btnEnviar.TabIndex = 19;
            btnEnviar.Text = "Enviar";
            btnEnviar.UseVisualStyleBackColor = false;
            btnEnviar.Click += btnEnviar_Click;
            // 
            // btnConectarAPN
            // 
            btnConectarAPN.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnConectarAPN.BackColor = SystemColors.ControlDark;
            btnConectarAPN.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point);
            btnConectarAPN.ForeColor = Color.Black;
            btnConectarAPN.Location = new Point(12, 451);
            btnConectarAPN.Name = "btnConectarAPN";
            btnConectarAPN.Size = new Size(271, 40);
            btnConectarAPN.TabIndex = 20;
            btnConectarAPN.Text = "Conectar APN";
            btnConectarAPN.UseVisualStyleBackColor = false;
            btnConectarAPN.Click += btnConectarAPN_Click;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label9.ForeColor = Color.FromArgb(64, 64, 64);
            label9.Location = new Point(12, 400);
            label9.Name = "label9";
            label9.Size = new Size(96, 15);
            label9.TabIndex = 21;
            label9.Text = "Server URL / IP:";
            // 
            // txtServerURL
            // 
            txtServerURL.Location = new Point(117, 392);
            txtServerURL.Name = "txtServerURL";
            txtServerURL.Size = new Size(95, 23);
            txtServerURL.TabIndex = 22;
            // 
            // btnConectarServer
            // 
            btnConectarServer.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnConectarServer.BackColor = SystemColors.ControlDark;
            btnConectarServer.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point);
            btnConectarServer.ForeColor = Color.Black;
            btnConectarServer.Location = new Point(12, 497);
            btnConectarServer.Name = "btnConectarServer";
            btnConectarServer.Size = new Size(271, 40);
            btnConectarServer.TabIndex = 23;
            btnConectarServer.Text = "Conectar con Server";
            btnConectarServer.UseVisualStyleBackColor = false;
            btnConectarServer.Click += btnConectarServer_Click;
            // 
            // numIP
            // 
            numIP.Location = new Point(218, 392);
            numIP.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numIP.Name = "numIP";
            numIP.Size = new Size(65, 23);
            numIP.TabIndex = 24;
            // 
            // btnRestart
            // 
            btnRestart.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnRestart.BackColor = SystemColors.ControlDark;
            btnRestart.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point);
            btnRestart.ForeColor = Color.Black;
            btnRestart.Location = new Point(12, 663);
            btnRestart.Name = "btnRestart";
            btnRestart.Size = new Size(271, 40);
            btnRestart.TabIndex = 25;
            btnRestart.Text = "Restart";
            btnRestart.UseVisualStyleBackColor = false;
            btnRestart.Click += btnRestart_Click;
            // 
            // btnStatus
            // 
            btnStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnStatus.BackColor = SystemColors.ControlDark;
            btnStatus.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point);
            btnStatus.ForeColor = Color.Black;
            btnStatus.Location = new Point(12, 617);
            btnStatus.Name = "btnStatus";
            btnStatus.Size = new Size(271, 40);
            btnStatus.TabIndex = 26;
            btnStatus.Text = "Get Status";
            btnStatus.UseVisualStyleBackColor = false;
            btnStatus.Click += btnStatus_Click;
            // 
            // TerminalSIM800L
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(862, 715);
            Controls.Add(btnStatus);
            Controls.Add(btnRestart);
            Controls.Add(numIP);
            Controls.Add(btnConectarServer);
            Controls.Add(txtServerURL);
            Controls.Add(label9);
            Controls.Add(btnConectarAPN);
            Controls.Add(btnEnviar);
            Controls.Add(txtComando);
            Controls.Add(label8);
            Controls.Add(lblIP);
            Controls.Add(lblConectado);
            Controls.Add(lblCalidadSenial);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(linkActualizar);
            Controls.Add(listaPuertos);
            Controls.Add(btnConectar);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtPassword);
            Controls.Add(txtUsuario);
            Controls.Add(txtAPN);
            Controls.Add(txtTerminal);
            Controls.Add(btnEjecutar);
            Name = "TerminalSIM800L";
            Text = "Terminal SIM800L";
            Load += TerminalSIM800L_Load;
            ((System.ComponentModel.ISupportInitialize)numIP).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnEjecutar;
        private RichTextBox txtTerminal;
        private TextBox txtAPN;
        private TextBox txtUsuario;
        private TextBox txtPassword;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button btnConectar;
        private ListView listaPuertos;
        private ColumnHeader puerto;
        private LinkLabel linkActualizar;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label lblCalidadSenial;
        private Label lblConectado;
        private Label lblIP;
        private Label label8;
        private TextBox txtComando;
        private Button btnEnviar;
        private Button btnConectarAPN;
        private Label label9;
        private TextBox txtServerURL;
        private Button btnConectarServer;
        private NumericUpDown numIP;
        private Button btnRestart;
        private Button btnStatus;
    }
}