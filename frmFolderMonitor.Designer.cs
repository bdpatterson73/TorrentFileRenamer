namespace TorrentFileRenamer
{
    partial class frmFolderMonitor
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            groupBox1 = new GroupBox();
            lblWatchFolderStatus = new Label();
            btnBrowseWatchFolder = new Button();
            txtWatchFolder = new TextBox();
            label1 = new Label();
            groupBox2 = new GroupBox();
            lblDestinationStatus = new Label();
            btnBrowseDestination = new Button();
            txtDestinationPath = new TextBox();
            label2 = new Label();
            groupBox3 = new GroupBox();
            label5 = new Label();
            numStabilityDelay = new NumericUpDown();
            label4 = new Label();
            txtFileExtensions = new TextBox();
            label3 = new Label();
            chkAutoStart = new CheckBox();
            btnOK = new Button();
            btnCancel = new Button();
            btnTestConfiguration = new Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numStabilityDelay).BeginInit();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(lblWatchFolderStatus);
            groupBox1.Controls.Add(btnBrowseWatchFolder);
            groupBox1.Controls.Add(txtWatchFolder);
            groupBox1.Controls.Add(label1);
            groupBox1.Location = new Point(14, 16);
            groupBox1.Margin = new Padding(3, 4, 3, 4);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(3, 4, 3, 4);
            groupBox1.Size = new Size(640, 107);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Watch Folder (HandBrake Output)";
            // 
            // lblWatchFolderStatus
            // 
            lblWatchFolderStatus.AutoSize = true;
            lblWatchFolderStatus.Location = new Point(7, 73);
            lblWatchFolderStatus.Name = "lblWatchFolderStatus";
            lblWatchFolderStatus.Size = new Size(0, 20);
            lblWatchFolderStatus.TabIndex = 3;
            // 
            // btnBrowseWatchFolder
            // 
            btnBrowseWatchFolder.Location = new Point(547, 50);
            btnBrowseWatchFolder.Margin = new Padding(3, 4, 3, 4);
            btnBrowseWatchFolder.Name = "btnBrowseWatchFolder";
            btnBrowseWatchFolder.Size = new Size(86, 31);
            btnBrowseWatchFolder.TabIndex = 2;
            btnBrowseWatchFolder.Text = "Browse...";
            btnBrowseWatchFolder.UseVisualStyleBackColor = true;
            btnBrowseWatchFolder.Click += btnBrowseWatchFolder_Click;
            // 
            // txtWatchFolder
            // 
            txtWatchFolder.Location = new Point(7, 50);
            txtWatchFolder.Margin = new Padding(3, 4, 3, 4);
            txtWatchFolder.Name = "txtWatchFolder";
            txtWatchFolder.Size = new Size(533, 27);
            txtWatchFolder.TabIndex = 1;
            txtWatchFolder.TextChanged += txtWatchFolder_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(7, 26);
            label1.Name = "label1";
            label1.Size = new Size(350, 20);
            label1.TabIndex = 0;
            label1.Text = "Folder to monitor for completed video conversions:";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(lblDestinationStatus);
            groupBox2.Controls.Add(btnBrowseDestination);
            groupBox2.Controls.Add(txtDestinationPath);
            groupBox2.Controls.Add(label2);
            groupBox2.Location = new Point(14, 131);
            groupBox2.Margin = new Padding(3, 4, 3, 4);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(3, 4, 3, 4);
            groupBox2.Size = new Size(640, 107);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Destination Folder (Plex Server)";
            // 
            // lblDestinationStatus
            // 
            lblDestinationStatus.AutoSize = true;
            lblDestinationStatus.Location = new Point(7, 73);
            lblDestinationStatus.Name = "lblDestinationStatus";
            lblDestinationStatus.Size = new Size(0, 20);
            lblDestinationStatus.TabIndex = 3;
            // 
            // btnBrowseDestination
            // 
            btnBrowseDestination.Location = new Point(547, 50);
            btnBrowseDestination.Margin = new Padding(3, 4, 3, 4);
            btnBrowseDestination.Name = "btnBrowseDestination";
            btnBrowseDestination.Size = new Size(86, 31);
            btnBrowseDestination.TabIndex = 2;
            btnBrowseDestination.Text = "Browse...";
            btnBrowseDestination.UseVisualStyleBackColor = true;
            btnBrowseDestination.Click += btnBrowseDestination_Click;
            // 
            // txtDestinationPath
            // 
            txtDestinationPath.Location = new Point(7, 50);
            txtDestinationPath.Margin = new Padding(3, 4, 3, 4);
            txtDestinationPath.Name = "txtDestinationPath";
            txtDestinationPath.Size = new Size(533, 27);
            txtDestinationPath.TabIndex = 1;
            txtDestinationPath.Text = "Z:\\TV";
            txtDestinationPath.TextChanged += txtDestinationPath_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(7, 26);
            label2.Name = "label2";
            label2.Size = new Size(291, 20);
            label2.TabIndex = 0;
            label2.Text = "Destination folder for processed TV shows:";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(label5);
            groupBox3.Controls.Add(numStabilityDelay);
            groupBox3.Controls.Add(label4);
            groupBox3.Controls.Add(txtFileExtensions);
            groupBox3.Controls.Add(label3);
            groupBox3.Location = new Point(14, 245);
            groupBox3.Margin = new Padding(3, 4, 3, 4);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new Padding(3, 4, 3, 4);
            groupBox3.Size = new Size(640, 133);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "Monitoring Settings";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(149, 87);
            label5.Name = "label5";
            label5.Size = new Size(370, 20);
            label5.TabIndex = 4;
            label5.Text = "seconds (wait time to ensure file is completely written)";
            // 
            // numStabilityDelay
            // 
            numStabilityDelay.Location = new Point(7, 84);
            numStabilityDelay.Margin = new Padding(3, 4, 3, 4);
            numStabilityDelay.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
            numStabilityDelay.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            numStabilityDelay.Name = "numStabilityDelay";
            numStabilityDelay.Size = new Size(135, 27);
            numStabilityDelay.TabIndex = 3;
            numStabilityDelay.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(7, 60);
            label4.Name = "label4";
            label4.Size = new Size(106, 20);
            label4.TabIndex = 2;
            label4.Text = "Stability delay:";
            // 
            // txtFileExtensions
            // 
            txtFileExtensions.Location = new Point(7, 25);
            txtFileExtensions.Margin = new Padding(3, 4, 3, 4);
            txtFileExtensions.Name = "txtFileExtensions";
            txtFileExtensions.Size = new Size(626, 27);
            txtFileExtensions.TabIndex = 1;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(7, 1);
            label3.Name = "label3";
            label3.Size = new Size(335, 20);
            label3.TabIndex = 0;
            label3.Text = "File extensions to monitor (semicolon separated):";
            // 
            // chkAutoStart
            // 
            chkAutoStart.AutoSize = true;
            chkAutoStart.Location = new Point(21, 387);
            chkAutoStart.Margin = new Padding(3, 4, 3, 4);
            chkAutoStart.Name = "chkAutoStart";
            chkAutoStart.Size = new Size(374, 24);
            chkAutoStart.TabIndex = 3;
            chkAutoStart.Text = "Start monitoring automatically when program starts";
            chkAutoStart.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            btnOK.Location = new Point(475, 427);
            btnOK.Margin = new Padding(3, 4, 3, 4);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(86, 31);
            btnOK.TabIndex = 4;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(568, 427);
            btnCancel.Margin = new Padding(3, 4, 3, 4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(86, 31);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnTestConfiguration
            // 
            btnTestConfiguration.Location = new Point(14, 427);
            btnTestConfiguration.Margin = new Padding(3, 4, 3, 4);
            btnTestConfiguration.Name = "btnTestConfiguration";
            btnTestConfiguration.Size = new Size(137, 31);
            btnTestConfiguration.TabIndex = 6;
            btnTestConfiguration.Text = "Test Configuration";
            btnTestConfiguration.UseVisualStyleBackColor = true;
            btnTestConfiguration.Click += btnTestConfiguration_Click;
            // 
            // frmFolderMonitor
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(667, 473);
            Controls.Add(btnTestConfiguration);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(chkAutoStart);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmFolderMonitor";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Auto-Monitor HandBrake Output";
            Load += frmFolderMonitor_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numStabilityDelay).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private GroupBox groupBox1;
        private Label lblWatchFolderStatus;
        private Button btnBrowseWatchFolder;
        private TextBox txtWatchFolder;
        private Label label1;
        private GroupBox groupBox2;
        private Label lblDestinationStatus;
        private Button btnBrowseDestination;
        private TextBox txtDestinationPath;
        private Label label2;
        private GroupBox groupBox3;
        private Label label5;
        private NumericUpDown numStabilityDelay;
        private Label label4;
        private TextBox txtFileExtensions;
        private Label label3;
        private CheckBox chkAutoStart;
        private Button btnOK;
        private Button btnCancel;
        private Button btnTestConfiguration;
    }
}