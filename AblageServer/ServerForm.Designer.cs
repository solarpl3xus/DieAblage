namespace AblageServer
{
    partial class ServerForm
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
            this.distributeFileButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // distributeFileButton
            // 
            this.distributeFileButton.Location = new System.Drawing.Point(12, 12);
            this.distributeFileButton.Name = "distributeFileButton";
            this.distributeFileButton.Size = new System.Drawing.Size(260, 23);
            this.distributeFileButton.TabIndex = 0;
            this.distributeFileButton.Text = "Distribute File";
            this.distributeFileButton.UseVisualStyleBackColor = true;
            this.distributeFileButton.Click += new System.EventHandler(this.OnDistributeFileButtonClick);
            // 
            // ServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.distributeFileButton);
            this.Name = "ServerForm";
            this.Text = "Ablagen Server";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button distributeFileButton;
    }
}

