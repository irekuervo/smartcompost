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
            SuspendLayout();
            // 
            // btnEjecutar
            // 
            btnEjecutar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnEjecutar.BackColor = SystemColors.ControlDark;
            btnEjecutar.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point);
            btnEjecutar.ForeColor = Color.Black;
            btnEjecutar.Location = new Point(12, 395);
            btnEjecutar.Name = "btnEjecutar";
            btnEjecutar.Size = new Size(228, 40);
            btnEjecutar.TabIndex = 0;
            btnEjecutar.Text = "Dale gas nene";
            btnEjecutar.UseVisualStyleBackColor = false;
            btnEjecutar.Click += btnEjecutar_Click;
            // 
            // txtTerminal
            // 
            txtTerminal.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            txtTerminal.Location = new Point(246, 12);
            txtTerminal.Name = "txtTerminal";
            txtTerminal.Size = new Size(517, 423);
            txtTerminal.TabIndex = 1;
            txtTerminal.Text = "";
            // 
            // txtAPN
            // 
            txtAPN.Location = new Point(79, 12);
            txtAPN.Name = "txtAPN";
            txtAPN.Size = new Size(161, 23);
            txtAPN.TabIndex = 2;
            // 
            // txtUsuario
            // 
            txtUsuario.Location = new Point(79, 46);
            txtUsuario.Name = "txtUsuario";
            txtUsuario.Size = new Size(161, 23);
            txtUsuario.TabIndex = 3;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(79, 79);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(161, 23);
            txtPassword.TabIndex = 4;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label1.ForeColor = Color.FromArgb(64, 64, 64);
            label1.Location = new Point(12, 20);
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
            label2.Location = new Point(12, 54);
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
            label3.Location = new Point(12, 87);
            label3.Name = "label3";
            label3.Size = new Size(59, 15);
            label3.TabIndex = 7;
            label3.Text = "Password";
            // 
            // btnConectar
            // 
            btnConectar.Location = new Point(12, 244);
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
            listaPuertos.Location = new Point(12, 135);
            listaPuertos.MultiSelect = false;
            listaPuertos.Name = "listaPuertos";
            listaPuertos.Size = new Size(221, 103);
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
            linkActualizar.Location = new Point(175, 120);
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
            label4.Location = new Point(12, 290);
            label4.Name = "label4";
            label4.Size = new Size(81, 15);
            label4.TabIndex = 11;
            label4.Text = "Calidad Señal:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(12, 316);
            label5.Name = "label5";
            label5.Size = new Size(97, 15);
            label5.TabIndex = 12;
            label5.Text = "Conectado a red:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(12, 344);
            label6.Name = "label6";
            label6.Size = new Size(20, 15);
            label6.TabIndex = 13;
            label6.Text = "IP:";
            // 
            // lblCalidadSenial
            // 
            lblCalidadSenial.AutoSize = true;
            lblCalidadSenial.Location = new Point(127, 290);
            lblCalidadSenial.Name = "lblCalidadSenial";
            lblCalidadSenial.Size = new Size(12, 15);
            lblCalidadSenial.TabIndex = 14;
            lblCalidadSenial.Text = "-";
            // 
            // TerminalSIM800L
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(787, 447);
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
            Text = "Form1";
            Load += TerminalSIM800L_Load;
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
    }
}