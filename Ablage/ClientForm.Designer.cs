namespace Ablage
{
    partial class ClientForm
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
            this.serverStatussplitContainer = new System.Windows.Forms.SplitContainer();
            this.onlineClientsBox = new System.Windows.Forms.ListBox();
            this.serverStatusLabel = new System.Windows.Forms.Label();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.uploadLabel = new System.Windows.Forms.Label();
            this.uploadProgressBar = new System.Windows.Forms.ProgressBar();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.downloadLabel = new System.Windows.Forms.Label();
            this.downloadProgressBar = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.serverStatussplitContainer)).BeginInit();
            this.serverStatussplitContainer.Panel1.SuspendLayout();
            this.serverStatussplitContainer.Panel2.SuspendLayout();
            this.serverStatussplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.SuspendLayout();
            // 
            // sendFileButton
            // 
            this.sendFileButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sendFileButton.Location = new System.Drawing.Point(0, 0);
            this.sendFileButton.Name = "sendFileButton";
            this.sendFileButton.Size = new System.Drawing.Size(320, 62);
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
            this.splitContainer1.Panel1.Controls.Add(this.serverStatussplitContainer);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(320, 374);
            this.splitContainer1.SplitterDistance = 63;
            this.splitContainer1.TabIndex = 1;
            // 
            // serverStatussplitContainer
            // 
            this.serverStatussplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.serverStatussplitContainer.Location = new System.Drawing.Point(0, 0);
            this.serverStatussplitContainer.Name = "serverStatussplitContainer";
            // 
            // serverStatussplitContainer.Panel1
            // 
            this.serverStatussplitContainer.Panel1.Controls.Add(this.onlineClientsBox);
            // 
            // serverStatussplitContainer.Panel2
            // 
            this.serverStatussplitContainer.Panel2.Controls.Add(this.serverStatusLabel);
            this.serverStatussplitContainer.Size = new System.Drawing.Size(320, 63);
            this.serverStatussplitContainer.SplitterDistance = 144;
            this.serverStatussplitContainer.TabIndex = 1;
            // 
            // onlineClientsBox
            // 
            this.onlineClientsBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.onlineClientsBox.FormattingEnabled = true;
            this.onlineClientsBox.Location = new System.Drawing.Point(0, 0);
            this.onlineClientsBox.Name = "onlineClientsBox";
            this.onlineClientsBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.onlineClientsBox.Size = new System.Drawing.Size(144, 63);
            this.onlineClientsBox.TabIndex = 0;
            // 
            // serverStatusLabel
            // 
            this.serverStatusLabel.BackColor = System.Drawing.Color.Red;
            this.serverStatusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.serverStatusLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.serverStatusLabel.Location = new System.Drawing.Point(0, 0);
            this.serverStatusLabel.Name = "serverStatusLabel";
            this.serverStatusLabel.Size = new System.Drawing.Size(172, 63);
            this.serverStatusLabel.TabIndex = 0;
            this.serverStatusLabel.Text = "Server cannot be reached";
            this.serverStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer3);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.sendFileButton);
            this.splitContainer2.Size = new System.Drawing.Size(320, 307);
            this.splitContainer2.SplitterDistance = 241;
            this.splitContainer2.TabIndex = 1;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.uploadLabel);
            this.splitContainer3.Panel1.Controls.Add(this.uploadProgressBar);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.splitContainer4);
            this.splitContainer3.Size = new System.Drawing.Size(320, 241);
            this.splitContainer3.SplitterDistance = 44;
            this.splitContainer3.TabIndex = 1;
            // 
            // uploadLabel
            // 
            this.uploadLabel.AutoSize = true;
            this.uploadLabel.Location = new System.Drawing.Point(3, -1);
            this.uploadLabel.Name = "uploadLabel";
            this.uploadLabel.Size = new System.Drawing.Size(41, 13);
            this.uploadLabel.TabIndex = 1;
            this.uploadLabel.Text = "Upload";
            // 
            // uploadProgressBar
            // 
            this.uploadProgressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.uploadProgressBar.Location = new System.Drawing.Point(0, 16);
            this.uploadProgressBar.Name = "uploadProgressBar";
            this.uploadProgressBar.Size = new System.Drawing.Size(320, 28);
            this.uploadProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.uploadProgressBar.TabIndex = 0;
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.downloadLabel);
            this.splitContainer4.Panel1.Controls.Add(this.downloadProgressBar);
            this.splitContainer4.Size = new System.Drawing.Size(320, 193);
            this.splitContainer4.SplitterDistance = 53;
            this.splitContainer4.TabIndex = 0;
            // 
            // downloadLabel
            // 
            this.downloadLabel.AutoSize = true;
            this.downloadLabel.Location = new System.Drawing.Point(3, 9);
            this.downloadLabel.Name = "downloadLabel";
            this.downloadLabel.Size = new System.Drawing.Size(55, 13);
            this.downloadLabel.TabIndex = 2;
            this.downloadLabel.Text = "Download";
            // 
            // downloadProgressBar
            // 
            this.downloadProgressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.downloadProgressBar.Location = new System.Drawing.Point(0, 25);
            this.downloadProgressBar.Name = "downloadProgressBar";
            this.downloadProgressBar.Size = new System.Drawing.Size(320, 28);
            this.downloadProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.downloadProgressBar.TabIndex = 1;
            // 
            // ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 374);
            this.Controls.Add(this.splitContainer1);
            this.KeyPreview = true;
            this.Name = "ClientForm";
            this.Text = "Die Ablage";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.serverStatussplitContainer.Panel1.ResumeLayout(false);
            this.serverStatussplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.serverStatussplitContainer)).EndInit();
            this.serverStatussplitContainer.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel1.PerformLayout();
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button sendFileButton;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox onlineClientsBox;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.ProgressBar uploadProgressBar;
        private System.Windows.Forms.Label uploadLabel;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.Label downloadLabel;
        private System.Windows.Forms.ProgressBar downloadProgressBar;
        private System.Windows.Forms.SplitContainer serverStatussplitContainer;
        private System.Windows.Forms.Label serverStatusLabel;
    }
}

