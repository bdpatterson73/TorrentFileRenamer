namespace TorrentFileRenamer
{
    partial class frmScanMovies
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
            btnBrowseVideoPath = new Button();
            btnBrowse = new Button();
            txtOutputDirectory = new TextBox();
            txtExtension = new TextBox();
            label3 = new Label();
            label2 = new Label();
            txtVideoPath = new TextBox();
            label1 = new Label();
            btnCancel = new Button();
            btnOK = new Button();
            SuspendLayout();
            // 
            // btnBrowseVideoPath
            // 
            btnBrowseVideoPath.Location = new Point(763, 43);
            btnBrowseVideoPath.Name = "btnBrowseVideoPath";
            btnBrowseVideoPath.Size = new Size(34, 32);
            btnBrowseVideoPath.TabIndex = 17;
            btnBrowseVideoPath.Text = "...";
            btnBrowseVideoPath.UseVisualStyleBackColor = true;
            btnBrowseVideoPath.Click += btnBrowseVideoPath_Click;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(763, 205);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(34, 32);
            btnBrowse.TabIndex = 16;
            btnBrowse.Text = "...";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // txtOutputDirectory
            // 
            txtOutputDirectory.Location = new Point(16, 205);
            txtOutputDirectory.Name = "txtOutputDirectory";
            txtOutputDirectory.Size = new Size(740, 31);
            txtOutputDirectory.TabIndex = 15;
            txtOutputDirectory.Text = "U:\\Movies";
            // 
            // txtExtension
            // 
            txtExtension.Location = new Point(16, 127);
            txtExtension.Name = "txtExtension";
            txtExtension.Size = new Size(150, 31);
            txtExtension.TabIndex = 14;
            txtExtension.Text = ".mp4";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(16, 178);
            label3.Name = "label3";
            label3.Size = new Size(146, 25);
            label3.TabIndex = 13;
            label3.Text = "Output Directory";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(16, 98);
            label2.Name = "label2";
            label2.Size = new Size(87, 25);
            label2.TabIndex = 12;
            label2.Text = "Extension";
            // 
            // txtVideoPath
            // 
            txtVideoPath.Location = new Point(16, 43);
            txtVideoPath.Name = "txtVideoPath";
            txtVideoPath.Size = new Size(740, 31);
            txtVideoPath.TabIndex = 11;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(16, 15);
            label1.Name = "label1";
            label1.Size = new Size(101, 25);
            label1.TabIndex = 10;
            label1.Text = "Video Path:";
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(569, 278);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(111, 53);
            btnCancel.TabIndex = 19;
            btnCancel.Text = "&Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnOK
            // 
            btnOK.Location = new Point(686, 278);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(111, 53);
            btnOK.TabIndex = 18;
            btnOK.Text = "&OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // frmScanMovies
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(830, 348);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(btnBrowseVideoPath);
            Controls.Add(btnBrowse);
            Controls.Add(txtOutputDirectory);
            Controls.Add(txtExtension);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(txtVideoPath);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4, 5, 4, 5);
            Name = "frmScanMovies";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Scan Movies";
            Load += frmScanMovies_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnBrowseVideoPath;
        private Button btnBrowse;
        private TextBox txtOutputDirectory;
        private TextBox txtExtension;
        private Label label3;
        private Label label2;
        private TextBox txtVideoPath;
        private Label label1;
        private Button btnCancel;
        private Button btnOK;
    }
}