namespace TorrentFileRenamer
{
    partial class frmScanOptions
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
            label1 = new Label();
            txtVideoPath = new TextBox();
            label2 = new Label();
            label3 = new Label();
            txtExtension = new TextBox();
            txtOutputDirectory = new TextBox();
            btnOK = new Button();
            btnCancel = new Button();
            btnBrowse = new Button();
            btnBrowseVideoPath = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(19, 13);
            label1.Name = "label1";
            label1.Size = new Size(101, 25);
            label1.TabIndex = 0;
            label1.Text = "Video Path:";
            // 
            // txtVideoPath
            // 
            txtVideoPath.Location = new Point(19, 42);
            txtVideoPath.Name = "txtVideoPath";
            txtVideoPath.Size = new Size(740, 31);
            txtVideoPath.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(19, 97);
            label2.Name = "label2";
            label2.Size = new Size(87, 25);
            label2.TabIndex = 2;
            label2.Text = "Extension";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(19, 177);
            label3.Name = "label3";
            label3.Size = new Size(146, 25);
            label3.TabIndex = 3;
            label3.Text = "Output Directory";
            // 
            // txtExtension
            // 
            txtExtension.Location = new Point(19, 125);
            txtExtension.Name = "txtExtension";
            txtExtension.Size = new Size(150, 31);
            txtExtension.TabIndex = 4;
            txtExtension.Text = ".mp4";
            // 
            // txtOutputDirectory
            // 
            txtOutputDirectory.Location = new Point(19, 203);
            txtOutputDirectory.Name = "txtOutputDirectory";
            txtOutputDirectory.Size = new Size(740, 31);
            txtOutputDirectory.TabIndex = 5;
            txtOutputDirectory.Text = "U:\\TV";
            // 
            // btnOK
            // 
            btnOK.Location = new Point(689, 277);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(111, 53);
            btnOK.TabIndex = 6;
            btnOK.Text = "&OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(571, 277);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(111, 53);
            btnCancel.TabIndex = 7;
            btnCancel.Text = "&Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(766, 203);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(34, 32);
            btnBrowse.TabIndex = 8;
            btnBrowse.Text = "...";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // btnBrowseVideoPath
            // 
            btnBrowseVideoPath.Location = new Point(766, 42);
            btnBrowseVideoPath.Name = "btnBrowseVideoPath";
            btnBrowseVideoPath.Size = new Size(34, 32);
            btnBrowseVideoPath.TabIndex = 9;
            btnBrowseVideoPath.Text = "...";
            btnBrowseVideoPath.UseVisualStyleBackColor = true;
            btnBrowseVideoPath.Click += btnBrowseVideoPath_Click;
            // 
            // frmScanOptions
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(813, 343);
            Controls.Add(btnBrowseVideoPath);
            Controls.Add(btnBrowse);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(txtOutputDirectory);
            Controls.Add(txtExtension);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(txtVideoPath);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "frmScanOptions";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Scan Options";
            Load += frmScanOptions_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtVideoPath;
        private Label label2;
        private Label label3;
        private TextBox txtExtension;
        private TextBox txtOutputDirectory;
        private Button btnOK;
        private Button btnCancel;
        private Button btnBrowse;
        private Button btnBrowseVideoPath;
    }
}