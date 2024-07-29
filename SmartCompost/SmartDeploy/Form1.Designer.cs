namespace SmartDeploy
{
    partial class Form1
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
            comboBoxComPorts = new ComboBox();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(268, 93);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 0;
            button1.Text = "Deploy";
            button1.UseVisualStyleBackColor = true;
            button1.Click += buttonExecute_Click;
            // 
            // comboBoxComPorts
            // 
            comboBoxComPorts.FormattingEnabled = true;
            comboBoxComPorts.Location = new Point(127, 94);
            comboBoxComPorts.Name = "comboBoxComPorts";
            comboBoxComPorts.Size = new Size(121, 23);
            comboBoxComPorts.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(comboBoxComPorts);
            Controls.Add(button1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private Button button1;
        private ComboBox comboBoxComPorts;
    }
}
