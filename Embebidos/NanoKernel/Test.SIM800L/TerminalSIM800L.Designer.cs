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
            button1 = new Button();
            terminal = new RichTextBox();
            txtAPN = new TextBox();
            txtUsuario = new TextBox();
            txtPassword = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button1.BackColor = SystemColors.ControlDark;
            button1.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point);
            button1.ForeColor = Color.Black;
            button1.Location = new Point(12, 395);
            button1.Name = "button1";
            button1.Size = new Size(228, 40);
            button1.TabIndex = 0;
            button1.Text = "Dale gas nene";
            button1.UseVisualStyleBackColor = false;
            // 
            // terminal
            // 
            terminal.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            terminal.Location = new Point(246, 12);
            terminal.Name = "terminal";
            terminal.Size = new Size(517, 423);
            terminal.TabIndex = 1;
            terminal.Text = "";
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
            // TerminalSIM800L
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(787, 447);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtPassword);
            Controls.Add(txtUsuario);
            Controls.Add(txtAPN);
            Controls.Add(terminal);
            Controls.Add(button1);
            Name = "TerminalSIM800L";
            Text = "Form1";
            Load += TerminalSIM800L_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private RichTextBox terminal;
        private TextBox txtAPN;
        private TextBox txtUsuario;
        private TextBox txtPassword;
        private Label label1;
        private Label label2;
        private Label label3;
    }
}