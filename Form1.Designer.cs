namespace TorrentFileRenamer
{
    partial class frmMain
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
            
            // Dispose folder monitoring service
            if (disposing)
            {
                _folderMonitorService?.Dispose();
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            menuStrip1 = new MenuStrip();
            miScan = new ToolStripMenuItem();
            miScanFiles = new ToolStripMenuItem();
            miProcess = new ToolStripMenuItem();
            miAutoMonitor = new ToolStripMenuItem();
            miConfigureAutoMonitor = new ToolStripMenuItem();
            miToggleMonitoring = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            miMonitoringStatus = new ToolStripMenuItem();
            editToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            miShowHelp = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            miAbout = new ToolStripMenuItem();
            lvFiles = new ListView();
            chFileName = new ColumnHeader();
            chNewName = new ColumnHeader();
            chShow = new ColumnHeader();
            chSeason = new ColumnHeader();
            chEpisode = new ColumnHeader();
            contextMenuStrip1 = new ContextMenuStrip(components);
            miRemove = new ToolStripMenuItem();
            clearToolStripMenuItem = new ToolStripMenuItem();
            statusStrip1 = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            statusProgress = new ToolStripProgressBar();
            toolStripContainer1 = new ToolStripContainer();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            lvMovies = new ListView();
            chMovieFileName = new ColumnHeader();
            chMovieNewFileNamePath = new ColumnHeader();
            chMovieName = new ColumnHeader();
            chMovieYear = new ColumnHeader();
            chComment = new ColumnHeader();
            cmsMovie = new ContextMenuStrip(components);
            msMovieRemove = new ToolStripMenuItem();
            msMovieClear = new ToolStripMenuItem();
            tabPage3 = new TabPage();
            lvMovieCleaner = new ListView();
            chmcFileNamePath = new ColumnHeader();
            chmcMovieTitle = new ColumnHeader();
            chmcYear = new ColumnHeader();
            chmcNewFileName = new ColumnHeader();
            chmcComment = new ColumnHeader();
            toolStrip1 = new ToolStrip();
            tsbScan = new ToolStripButton();
            tsbProcess = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripButton1 = new ToolStripButton();
            toolStripButton2 = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            tsbAutoMonitor = new ToolStripButton();
            toolStripSeparator5 = new ToolStripSeparator();
            toolStripButton3 = new ToolStripButton();
            toolStripButton4 = new ToolStripButton();
            tsbForceDateCheck = new ToolStripButton();
            chRemark = new ColumnHeader();
            menuStrip1.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            toolStripContainer1.ContentPanel.SuspendLayout();
            toolStripContainer1.TopToolStripPanel.SuspendLayout();
            toolStripContainer1.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            cmsMovie.SuspendLayout();
            tabPage3.SuspendLayout();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(24, 24);
            menuStrip1.Items.AddRange(new ToolStripItem[] { miScan, miAutoMonitor, editToolStripMenuItem, helpToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(4, 1, 0, 1);
            menuStrip1.Size = new Size(804, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // miScan
            // 
            miScan.DropDownItems.AddRange(new ToolStripItem[] { miScanFiles, miProcess });
            miScan.Name = "miScan";
            miScan.Size = new Size(37, 22);
            miScan.Text = "&File";
            // 
            // miScanFiles
            // 
            miScanFiles.Name = "miScanFiles";
            miScanFiles.Size = new Size(114, 22);
            miScanFiles.Text = "&Scan";
            miScanFiles.Click += miScanFiles_Click;
            // 
            // miProcess
            // 
            miProcess.Name = "miProcess";
            miProcess.Size = new Size(114, 22);
            miProcess.Text = "Process";
            miProcess.Click += miProcess_Click;
            // 
            // miAutoMonitor
            // 
            miAutoMonitor.DropDownItems.AddRange(new ToolStripItem[] { miConfigureAutoMonitor, miToggleMonitoring, toolStripSeparator3, miMonitoringStatus });
            miAutoMonitor.Name = "miAutoMonitor";
            miAutoMonitor.Size = new Size(88, 22);
            miAutoMonitor.Text = "&Auto-Monitor";
            // 
            // miConfigureAutoMonitor
            // 
            miConfigureAutoMonitor.Name = "miConfigureAutoMonitor";
            miConfigureAutoMonitor.Size = new Size(180, 22);
            miConfigureAutoMonitor.Text = "&Configure Auto-Monitor...";
            miConfigureAutoMonitor.Click += miAutoMonitor_Click;
            // 
            // miToggleMonitoring
            // 
            miToggleMonitoring.Name = "miToggleMonitoring";
            miToggleMonitoring.Size = new Size(180, 22);
            miToggleMonitoring.Text = "&Start/Stop Monitoring";
            miToggleMonitoring.Click += miToggleMonitoring_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(177, 6);
            // 
            // miMonitoringStatus
            // 
            miMonitoringStatus.Name = "miMonitoringStatus";
            miMonitoringStatus.Size = new Size(180, 22);
            miMonitoringStatus.Text = "Monitoring &Status...";
            miMonitoringStatus.Click += miMonitoringStatus_Click;
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new Size(39, 22);
            editToolStripMenuItem.Text = "&Edit";
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { miShowHelp, toolStripSeparator4, miAbout });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(44, 22);
            helpToolStripMenuItem.Text = "&Help";
            // 
            // miShowHelp
            // 
            miShowHelp.Name = "miShowHelp";
            miShowHelp.Size = new Size(107, 22);
            miShowHelp.Text = "&Help";
            miShowHelp.Click += miShowHelp_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(104, 6);
            // 
            // miAbout
            // 
            miAbout.Name = "miAbout";
            miAbout.Size = new Size(107, 22);
            miAbout.Text = "&About";
            miAbout.Click += miAbout_Click;
            // 
            // lvFiles
            // 
            lvFiles.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lvFiles.Columns.AddRange(new ColumnHeader[] { chFileName, chNewName, chShow, chSeason, chEpisode, chRemark });
            lvFiles.ContextMenuStrip = contextMenuStrip1;
            lvFiles.FullRowSelect = true;
            lvFiles.GridLines = true;
            lvFiles.Location = new Point(5, 5);
            lvFiles.Margin = new Padding(2);
            lvFiles.Name = "lvFiles";
            lvFiles.Size = new Size(780, 258);
            lvFiles.TabIndex = 1;
            lvFiles.UseCompatibleStateImageBehavior = false;
            lvFiles.View = View.Details;
            // 
            // chFileName
            // 
            chFileName.Text = "Filename";
            chFileName.Width = 200;
            // 
            // chNewName
            // 
            chNewName.Text = "New Name";
            chNewName.Width = 200;
            // 
            // chShow
            // 
            chShow.Text = "Show";
            chShow.Width = 120;
            // 
            // chSeason
            // 
            chSeason.Text = "Season";
            chSeason.Width = 80;
            // 
            // chEpisode
            // 
            chEpisode.Text = "Episode";
            chEpisode.Width = 80;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new Size(24, 24);
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { miRemove, clearToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(118, 48);
            // 
            // miRemove
            // 
            miRemove.Name = "miRemove";
            miRemove.Size = new Size(117, 22);
            miRemove.Text = "Remove";
            miRemove.Click += miRemove_Click;
            // 
            // clearToolStripMenuItem
            // 
            clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            clearToolStripMenuItem.Size = new Size(117, 22);
            clearToolStripMenuItem.Text = "Clear";
            clearToolStripMenuItem.Click += clearToolStripMenuItem_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { statusLabel, statusProgress });
            statusStrip1.Location = new Point(0, 371);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(804, 22);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            statusLabel.AutoSize = false;
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(500, 17);
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // statusProgress
            // 
            statusProgress.Name = "statusProgress";
            statusProgress.Size = new Size(100, 16);
            statusProgress.Step = 1;
            // 
            // toolStripContainer1
            // 
            toolStripContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // toolStripContainer1.ContentPanel
            // 
            toolStripContainer1.ContentPanel.Controls.Add(tabControl1);
            toolStripContainer1.ContentPanel.Size = new Size(804, 302);
            toolStripContainer1.Location = new Point(0, 27);
            toolStripContainer1.Name = "toolStripContainer1";
            toolStripContainer1.Size = new Size(804, 341);
            toolStripContainer1.TabIndex = 3;
            toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            toolStripContainer1.TopToolStripPanel.Controls.Add(toolStrip1);
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Location = new Point(3, 3);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(798, 296);
            tabControl1.TabIndex = 3;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(lvFiles);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(790, 268);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "TV";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(lvMovies);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(790, 268);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Movie";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // lvMovies
            // 
            lvMovies.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lvMovies.Columns.AddRange(new ColumnHeader[] { chMovieFileName, chMovieNewFileNamePath, chMovieName, chMovieYear, chComment });
            lvMovies.ContextMenuStrip = cmsMovie;
            lvMovies.FullRowSelect = true;
            lvMovies.GridLines = true;
            lvMovies.Location = new Point(6, 6);
            lvMovies.Name = "lvMovies";
            lvMovies.Size = new Size(778, 256);
            lvMovies.TabIndex = 2;
            lvMovies.UseCompatibleStateImageBehavior = false;
            lvMovies.View = View.Details;
            // 
            // chMovieFileName
            // 
            chMovieFileName.Text = "File Name";
            chMovieFileName.Width = 200;
            // 
            // chMovieNewFileNamePath
            // 
            chMovieNewFileNamePath.Text = "New Path";
            chMovieNewFileNamePath.Width = 120;
            // 
            // chMovieName
            // 
            chMovieName.Text = "Movie Name";
            chMovieName.Width = 120;
            // 
            // chMovieYear
            // 
            chMovieYear.Text = "Year";
            // 
            // chComment
            // 
            chComment.Text = "Comment";
            // 
            // cmsMovie
            // 
            cmsMovie.Items.AddRange(new ToolStripItem[] { msMovieRemove, msMovieClear });
            cmsMovie.Name = "cmsMovie";
            cmsMovie.Size = new Size(118, 48);
            // 
            // msMovieRemove
            // 
            msMovieRemove.Name = "msMovieRemove";
            msMovieRemove.Size = new Size(117, 22);
            msMovieRemove.Text = "Remove";
            msMovieRemove.Click += msMovieRemove_Click;
            // 
            // msMovieClear
            // 
            msMovieClear.Name = "msMovieClear";
            msMovieClear.Size = new Size(117, 22);
            msMovieClear.Text = "Clear";
            msMovieClear.Click += msMovieClear_Click;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(lvMovieCleaner);
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(790, 268);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Movie Cleaner";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // lvMovieCleaner
            // 
            lvMovieCleaner.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lvMovieCleaner.Columns.AddRange(new ColumnHeader[] { chmcFileNamePath, chmcMovieTitle, chmcYear, chmcNewFileName, chmcComment });
            lvMovieCleaner.Location = new Point(5, 3);
            lvMovieCleaner.Name = "lvMovieCleaner";
            lvMovieCleaner.Size = new Size(780, 262);
            lvMovieCleaner.TabIndex = 0;
            lvMovieCleaner.UseCompatibleStateImageBehavior = false;
            lvMovieCleaner.View = View.Details;
            // 
            // chmcFileNamePath
            // 
            chmcFileNamePath.Text = "File";
            chmcFileNamePath.Width = 200;
            // 
            // chmcMovieTitle
            // 
            chmcMovieTitle.Text = "Movie Title";
            chmcMovieTitle.Width = 200;
            // 
            // chmcYear
            // 
            chmcYear.Text = "Year";
            // 
            // chmcNewFileName
            // 
            chmcNewFileName.Text = "New File Name";
            chmcNewFileName.Width = 200;
            // 
            // chmcComment
            // 
            chmcComment.Text = "Comment";
            // 
            // toolStrip1
            // 
            toolStrip1.Dock = DockStyle.None;
            toolStrip1.ImageScalingSize = new Size(32, 32);
            toolStrip1.Items.AddRange(new ToolStripItem[] { tsbScan, tsbProcess, toolStripSeparator1, toolStripButton1, toolStripButton2, toolStripSeparator2, tsbAutoMonitor, toolStripSeparator5, toolStripButton3, toolStripButton4, tsbForceDateCheck });
            toolStrip1.Location = new Point(3, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(380, 39);
            toolStrip1.TabIndex = 0;
            // 
            // tsbScan
            // 
            tsbScan.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbScan.Image = (Image)resources.GetObject("tsbScan.Image");
            tsbScan.ImageTransparentColor = Color.Magenta;
            tsbScan.Name = "tsbScan";
            tsbScan.Size = new Size(36, 36);
            tsbScan.Text = "Scan TV";
            tsbScan.Click += tsbScan_Click;
            // 
            // tsbProcess
            // 
            tsbProcess.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbProcess.Image = (Image)resources.GetObject("tsbProcess.Image");
            tsbProcess.ImageTransparentColor = Color.Magenta;
            tsbProcess.Name = "tsbProcess";
            tsbProcess.Size = new Size(36, 36);
            tsbProcess.Text = "Process TV";
            tsbProcess.Click += tsbProcess_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 39);
            // 
            // toolStripButton1
            // 
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton1.Image = (Image)resources.GetObject("toolStripButton1.Image");
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(36, 36);
            toolStripButton1.Text = "Scan Movie";
            toolStripButton1.Click += toolStripButton1_Click;
            // 
            // toolStripButton2
            // 
            toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton2.Image = (Image)resources.GetObject("toolStripButton2.Image");
            toolStripButton2.ImageTransparentColor = Color.Magenta;
            toolStripButton2.Name = "toolStripButton2";
            toolStripButton2.Size = new Size(36, 36);
            toolStripButton2.Text = "Process Movie";
            toolStripButton2.Click += toolStripButton2_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 39);
            // 
            // tsbAutoMonitor
            // 
            tsbAutoMonitor.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tsbAutoMonitor.ImageTransparentColor = Color.Magenta;
            tsbAutoMonitor.Name = "tsbAutoMonitor";
            tsbAutoMonitor.Size = new Size(63, 36);
            tsbAutoMonitor.Text = "📁 Monitor";
            tsbAutoMonitor.ToolTipText = "Configure Auto-Monitor for HandBrake";
            tsbAutoMonitor.Click += tsbAutoMonitor_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(6, 39);
            // 
            // toolStripButton3
            // 
            toolStripButton3.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton3.Image = (Image)resources.GetObject("toolStripButton3.Image");
            toolStripButton3.ImageTransparentColor = Color.Magenta;
            toolStripButton3.Name = "toolStripButton3";
            toolStripButton3.Size = new Size(36, 36);
            toolStripButton3.Text = "Scan Movie Cleaner";
            toolStripButton3.Click += toolStripButton3_Click;
            // 
            // toolStripButton4
            // 
            toolStripButton4.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton4.Image = (Image)resources.GetObject("toolStripButton4.Image");
            toolStripButton4.ImageTransparentColor = Color.Magenta;
            toolStripButton4.Name = "toolStripButton4";
            toolStripButton4.Size = new Size(36, 36);
            toolStripButton4.Text = "Process Movie Cleaner";
            toolStripButton4.Click += toolStripButton4_Click;
            // 
            // tsbForceDateCheck
            // 
            tsbForceDateCheck.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbForceDateCheck.Image = (Image)resources.GetObject("tsbForceDateCheck.Image");
            tsbForceDateCheck.ImageTransparentColor = Color.Magenta;
            tsbForceDateCheck.Name = "tsbForceDateCheck";
            tsbForceDateCheck.Size = new Size(36, 36);
            tsbForceDateCheck.Text = "toolStripButton5";
            tsbForceDateCheck.Click += tsbForceDateCheck_Click;
            // 
            // chRemark
            // 
            chRemark.Text = "Remark";
            chRemark.Width = 250;
            // 
            // frmMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(804, 393);
            Controls.Add(toolStripContainer1);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            Margin = new Padding(2);
            Name = "frmMain";
            Text = "Torrent File Renamer";
            Load += frmMain_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            contextMenuStrip1.ResumeLayout(false);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            toolStripContainer1.ContentPanel.ResumeLayout(false);
            toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            toolStripContainer1.TopToolStripPanel.PerformLayout();
            toolStripContainer1.ResumeLayout(false);
            toolStripContainer1.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            cmsMovie.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem miScan;
        private ToolStripMenuItem miScanFiles;
        private ToolStripMenuItem miAutoMonitor;
        private ToolStripMenuItem miConfigureAutoMonitor;
        private ToolStripMenuItem miToggleMonitoring;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem miMonitoringStatus;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem miShowHelp;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem miAbout;
        private ListView lvFiles;
        private ColumnHeader chFileName;
        private ColumnHeader chNewName;
        private ColumnHeader chShow;
        private ColumnHeader chSeason;
        private ColumnHeader chEpisode;
        private ToolStripMenuItem miProcess;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem miRemove;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel statusLabel;
        private ToolStripProgressBar statusProgress;
        private ToolStripContainer toolStripContainer1;
        private ToolStrip toolStrip1;
        private ToolStripButton tsbScan;
        private ToolStripButton tsbProcess;
        private ToolStripMenuItem clearToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton toolStripButton1;
        private ToolStripButton toolStripButton2;
        private ListView lvMovies;
        private ColumnHeader chMovieFileName;
        private ColumnHeader chMovieNewFileNamePath;
        private ColumnHeader chMovieName;
        private ColumnHeader chMovieYear;
        private ColumnHeader chComment;
        private ContextMenuStrip cmsMovie;
        private ToolStripMenuItem msMovieRemove;
        private ToolStripMenuItem msMovieClear;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabPage tabPage3;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton toolStripButton3;
        private ToolStripButton toolStripButton4;
        private ListView lvMovieCleaner;
        private ColumnHeader chmcFileNamePath;
        private ColumnHeader chmcMovieTitle;
        private ColumnHeader chmcYear;
        private ColumnHeader chmcNewFileName;
        private ColumnHeader chmcComment;
        private ToolStripButton tsbForceDateCheck;
        private ColumnHeader chRemark;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripButton tsbAutoMonitor;
    }
}