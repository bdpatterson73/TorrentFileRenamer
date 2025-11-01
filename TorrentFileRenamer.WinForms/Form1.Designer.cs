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
            tsbScanMovies = new ToolStripButton();
            tsbProcessMovies = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            tsbAutoMonitor = new ToolStripButton();
            toolStripSeparator5 = new ToolStripSeparator();
            tsbScanMovieCleaner = new ToolStripButton();
            tsbProcessMovieCleaner = new ToolStripButton();
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
            menuStrip1.BackColor = Color.White;
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { miScan, miAutoMonitor, editToolStripMenuItem, helpToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(6, 2, 0, 2);
            menuStrip1.RenderMode = ToolStripRenderMode.Professional;
            menuStrip1.Size = new Size(1000, 28);
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
            lvFiles.Dock = DockStyle.Fill;
            lvFiles.BackColor = Color.White;
            lvFiles.BorderStyle = BorderStyle.None;
            lvFiles.Columns.AddRange(new ColumnHeader[] { chFileName, chNewName, chShow, chSeason, chEpisode, chRemark });
            lvFiles.ContextMenuStrip = contextMenuStrip1;
            lvFiles.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lvFiles.FullRowSelect = true;
            lvFiles.GridLines = true;
            lvFiles.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lvFiles.Location = new Point(0, 0);
            lvFiles.Margin = new Padding(0);
            lvFiles.Name = "lvFiles";
            lvFiles.OwnerDraw = false;
            lvFiles.Size = new Size(968, 426);
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
            statusStrip1.BackColor = Color.FromArgb(240, 240, 240);
            statusStrip1.Items.AddRange(new ToolStripItem[] { statusLabel, statusProgress });
            statusStrip1.Location = new Point(0, 578);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new Padding(1, 0, 14, 0);
            statusStrip1.RenderMode = ToolStripRenderMode.Professional;
            statusStrip1.Size = new Size(1000, 22);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(687, 17);
            statusLabel.Spring = true;
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
            toolStripContainer1.ContentPanel.BackColor = Color.FromArgb(245, 245, 245);
            toolStripContainer1.ContentPanel.Controls.Add(tabControl1);
            toolStripContainer1.ContentPanel.Padding = new Padding(4);
            toolStripContainer1.ContentPanel.Size = new Size(1000, 506);
            toolStripContainer1.Location = new Point(0, 28);
            toolStripContainer1.Name = "toolStripContainer1";
            toolStripContainer1.Size = new Size(1000, 550);
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
            tabControl1.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            tabControl1.Location = new Point(4, 4);
            tabControl1.Name = "tabControl1";
            tabControl1.Padding = new Point(12, 6);
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(992, 498);
            tabControl1.TabIndex = 3;
            // 
            // tabPage1
            //
            tabPage1.BackColor = Color.FromArgb(250, 250, 250);
            tabPage1.Controls.Add(lvFiles);
            tabPage1.Location = new Point(4, 28);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(0);
            tabPage1.Size = new Size(968, 426);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "📺 TV Shows";
            // 
            // tabPage2
            //
            tabPage2.BackColor = Color.FromArgb(250, 250, 250);
            tabPage2.Controls.Add(lvMovies);
            tabPage2.Location = new Point(4, 28);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(0);
            tabPage2.Size = new Size(968, 426);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "🎬 Movies";
            // 
            // lvMovies
            //
            lvMovies.Dock = DockStyle.Fill;
            lvMovies.BackColor = Color.White;
            lvMovies.BorderStyle = BorderStyle.None;
            lvMovies.Columns.AddRange(new ColumnHeader[] { chMovieFileName, chMovieNewFileNamePath, chMovieName, chMovieYear, chComment });
            lvMovies.ContextMenuStrip = cmsMovie;
            lvMovies.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lvMovies.FullRowSelect = true;
            lvMovies.GridLines = true;
            lvMovies.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lvMovies.Location = new Point(0, 0);
            lvMovies.Margin = new Padding(0);
            lvMovies.Name = "lvMovies";
            lvMovies.Size = new Size(968, 426);
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
            tabPage3.BackColor = Color.FromArgb(250, 250, 250);
            tabPage3.Controls.Add(lvMovieCleaner);
            tabPage3.Location = new Point(4, 28);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(0);
            tabPage3.Size = new Size(968, 426);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "🧹 Movie Cleaner";
            // 
            // lvMovieCleaner
            //
            lvMovieCleaner.Dock = DockStyle.Fill;
            lvMovieCleaner.BackColor = Color.White;
            lvMovieCleaner.BorderStyle = BorderStyle.None;
            lvMovieCleaner.Columns.AddRange(new ColumnHeader[] { chmcFileNamePath, chmcMovieTitle, chmcYear, chmcNewFileName, chmcComment });
            lvMovieCleaner.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lvMovieCleaner.FullRowSelect = true;
            lvMovieCleaner.GridLines = true;
            lvMovieCleaner.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lvMovieCleaner.Location = new Point(0, 0);
            lvMovieCleaner.Margin = new Padding(0);
            lvMovieCleaner.Name = "lvMovieCleaner";
            lvMovieCleaner.Size = new Size(968, 426);
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
            toolStrip1.BackColor = Color.White;
            toolStrip1.Dock = DockStyle.None;
            toolStrip1.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip1.ImageScalingSize = new Size(24, 24);
            toolStrip1.Items.AddRange(new ToolStripItem[] { tsbScan, tsbProcess, toolStripSeparator1, tsbScanMovies, tsbProcessMovies, toolStripSeparator2, tsbAutoMonitor, toolStripSeparator5, tsbScanMovieCleaner, tsbProcessMovieCleaner, tsbForceDateCheck });
            toolStrip1.Location = new Point(8, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Padding = new Padding(8, 4, 1, 4);
            toolStrip1.RenderMode = ToolStripRenderMode.Professional;
            toolStrip1.Size = new Size(502, 44);
            toolStrip1.TabIndex = 0;
            // 
            // tsbScan
            //
            tsbScan.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            tsbScan.Image = (Image)resources.GetObject("tsbScan.Image");
            tsbScan.ImageTransparentColor = Color.Magenta;
            tsbScan.Name = "tsbScan";
            tsbScan.Size = new Size(72, 36);
            tsbScan.Text = "Scan TV";
            tsbScan.ToolTipText = "Scan for TV show files";
            tsbScan.Click += tsbScan_Click;
            // 
            // tsbProcess
            //
            tsbProcess.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            tsbProcess.Image = (Image)resources.GetObject("tsbProcess.Image");
            tsbProcess.ImageTransparentColor = Color.Magenta;
            tsbProcess.Name = "tsbProcess";
            tsbProcess.Size = new Size(82, 36);
            tsbProcess.Text = "Process TV";
            tsbProcess.ToolTipText = "Process scanned TV files";
            tsbProcess.Click += miProcess_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 39);
            // 
            // tsbScanMovies
            //
            tsbScanMovies.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            tsbScanMovies.Image = (Image)resources.GetObject("toolStripButton1.Image");
            tsbScanMovies.ImageTransparentColor = Color.Magenta;
            tsbScanMovies.Name = "tsbScanMovies";
            tsbScanMovies.Size = new Size(94, 36);
            tsbScanMovies.Text = "Scan Movies";
            tsbScanMovies.ToolTipText = "Scan for movie files";
            tsbScanMovies.Click += toolStripButton1_Click;
            // 
            // tsbProcessMovies
            //
            tsbProcessMovies.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            tsbProcessMovies.Image = (Image)resources.GetObject("toolStripButton2.Image");
            tsbProcessMovies.ImageTransparentColor = Color.Magenta;
            tsbProcessMovies.Name = "tsbProcessMovies";
            tsbProcessMovies.Size = new Size(105, 36);
            tsbProcessMovies.Text = "Process Movies";
            tsbProcessMovies.ToolTipText = "Process scanned movie files";
            tsbProcessMovies.Click += toolStripButton2_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 39);
            // 
            // tsbAutoMonitor
            //
            tsbAutoMonitor.BackColor = Color.FromArgb(230, 240, 255);
            tsbAutoMonitor.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tsbAutoMonitor.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            tsbAutoMonitor.ForeColor = Color.FromArgb(60, 100, 180);
            tsbAutoMonitor.ImageTransparentColor = Color.Magenta;
            tsbAutoMonitor.Name = "tsbAutoMonitor";
            tsbAutoMonitor.Size = new Size(78, 36);
            tsbAutoMonitor.Text = "📁 Monitor";
            tsbAutoMonitor.ToolTipText = "Configure Auto-Monitor";
            tsbAutoMonitor.Click += tsbAutoMonitor_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(6, 39);
            // 
            // tsbScanMovieCleaner
            //
            tsbScanMovieCleaner.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            tsbScanMovieCleaner.Image = (Image)resources.GetObject("toolStripButton3.Image");
            tsbScanMovieCleaner.ImageTransparentColor = Color.Magenta;
            tsbScanMovieCleaner.Name = "tsbScanMovieCleaner";
            tsbScanMovieCleaner.Size = new Size(74, 36);
            tsbScanMovieCleaner.Text = "Scan Clean";
            tsbScanMovieCleaner.ToolTipText = "Scan files for cleaning";
            tsbScanMovieCleaner.Click += toolStripButton3_Click;
            // 
            // tsbProcessMovieCleaner
            //
            tsbProcessMovieCleaner.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            tsbProcessMovieCleaner.Image = (Image)resources.GetObject("toolStripButton4.Image");
            tsbProcessMovieCleaner.ImageTransparentColor = Color.Magenta;
            tsbProcessMovieCleaner.Name = "tsbProcessMovieCleaner";
            tsbProcessMovieCleaner.Size = new Size(92, 36);
            tsbProcessMovieCleaner.Text = "Process Clean";
            tsbProcessMovieCleaner.ToolTipText = "Process cleaned files";
            tsbProcessMovieCleaner.Click += toolStripButton4_Click;
            // 
            // tsbForceDateCheck
            //
            tsbForceDateCheck.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            tsbForceDateCheck.Image = (Image)resources.GetObject("tsbForceDateCheck.Image");
            tsbForceDateCheck.ImageTransparentColor = Color.Magenta;
            tsbForceDateCheck.Name = "tsbForceDateCheck";
            tsbForceDateCheck.Size = new Size(85, 36);
            tsbForceDateCheck.Text = "Date Check";
            tsbForceDateCheck.ToolTipText = "Force date check on files";
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
            BackColor = Color.FromArgb(245, 245, 245);
            ClientSize = new Size(1000, 600);
            Controls.Add(toolStripContainer1);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            MinimumSize = new Size(800, 500);
            Name = "frmMain";
            StartPosition = FormStartPosition.CenterScreen;
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
        private ToolStripButton tsbScanMovies;
        private ToolStripButton tsbProcessMovies;
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
        private ToolStripButton tsbScanMovieCleaner;
        private ToolStripButton tsbProcessMovieCleaner;
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