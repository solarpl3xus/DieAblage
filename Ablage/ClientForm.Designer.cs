namespace Ablage
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.sendFileButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // sendFileButton
            // 
            this.sendFileButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sendFileButton.Location = new System.Drawing.Point(0, 0);
            this.sendFileButton.Name = "sendFileButton";
            this.sendFileButton.Size = new System.Drawing.Size(210, 113);
            this.sendFileButton.TabIndex = 0;
            this.sendFileButton.Text = "Send File";
            this.sendFileButton.UseVisualStyleBackColor = true;
            this.sendFileButton.Click += new System.EventHandler(this.OnSendFileButtonClick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(210, 113);
            this.Controls.Add(this.sendFileButton);
            this.Name = "MainForm";
            this.Text = "Die Ablage";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button sendFileButton;
    }
}

