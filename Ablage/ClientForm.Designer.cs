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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.onlineClientsBox = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // sendFileButton
            // 
            this.sendFileButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sendFileButton.Location = new System.Drawing.Point(0, 0);
            this.sendFileButton.Name = "sendFileButton";
            this.sendFileButton.Size = new System.Drawing.Size(320, 204);
            this.sendFileButton.TabIndex = 0;
            this.sendFileButton.Text = "Send File";
            this.sendFileButton.UseVisualStyleBackColor = true;
            this.sendFileButton.Click += new System.EventHandler(this.OnSendFileButtonClick);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.onlineClientsBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.sendFileButton);
            this.splitContainer1.Size = new System.Drawing.Size(320, 374);
            this.splitContainer1.SplitterDistance = 166;
            this.splitContainer1.TabIndex = 1;
            // 
            // onlineClientsBox
            // 
            this.onlineClientsBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.onlineClientsBox.FormattingEnabled = true;
            this.onlineClientsBox.Location = new System.Drawing.Point(0, 0);
            this.onlineClientsBox.Name = "onlineClientsBox";
            this.onlineClientsBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.onlineClientsBox.Size = new System.Drawing.Size(320, 166);
            this.onlineClientsBox.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 374);
            this.Controls.Add(this.splitContainer1);
            this.Name = "MainForm";
            this.Text = "Die Ablage";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button sendFileButton;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox onlineClientsBox;
    }
}

