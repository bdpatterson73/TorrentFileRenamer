using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TorrentFileRenamer
{
    public partial class frmFolderMonitor : Form
    {
        public frmFolderMonitor()
        {
            InitializeComponent();
        }

        private void frmFolderMonitor_Load(object sender, EventArgs e)
        {
            // Set default values
            txtWatchFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            txtFileExtensions.Text = "*.mp4;*.mkv;*.avi;*.m4v";
            numStabilityDelay.Value = 30; // 30 seconds default
            txtDestinationPath.Text = "";
            chkAutoStart.Checked = false;
            numMaxLogEntries.Value = 20; // 20 log entries default
        }

        public string WatchFolder
        {
            get { return txtWatchFolder.Text.Trim(); }
            set { txtWatchFolder.Text = value; }
        }

        public string DestinationPath
        {
            get { return txtDestinationPath.Text.Trim(); }
            set { txtDestinationPath.Text = value; }
        }

        public string FileExtensions
        {
            get { return txtFileExtensions.Text.Trim(); }
            set { txtFileExtensions.Text = value; }
        }

        public int StabilityDelaySeconds
        {
            get { return (int)numStabilityDelay.Value; }
            set { numStabilityDelay.Value = Math.Max(5, Math.Min(300, value)); }
        }

        public bool AutoStartOnLoad
        {
            get { return chkAutoStart.Checked; }
            set { chkAutoStart.Checked = value; }
        }

        public int MaxAutoMonitorLogEntries
        {
            get { return (int)numMaxLogEntries.Value; }
            set { numMaxLogEntries.Value = Math.Max(5, Math.Min(50, value)); }
        }

        private void btnBrowseWatchFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select the folder to monitor for completed HandBrake conversions";
            fbd.ShowNewFolderButton = true;
            
            if (!string.IsNullOrWhiteSpace(txtWatchFolder.Text) && Directory.Exists(txtWatchFolder.Text))
            {
                fbd.SelectedPath = txtWatchFolder.Text;
            }
            
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                txtWatchFolder.Text = fbd.SelectedPath;
                ValidateWatchFolder();
            }
        }

        private void btnBrowseDestination_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select the destination folder for processed TV shows";
            fbd.ShowNewFolderButton = true;
            
            if (!string.IsNullOrWhiteSpace(txtDestinationPath.Text) && Directory.Exists(txtDestinationPath.Text))
            {
                fbd.SelectedPath = txtDestinationPath.Text;
            }
            
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                txtDestinationPath.Text = fbd.SelectedPath;
                ValidateDestinationFolder();
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (ValidateAllInputs())
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private bool ValidateAllInputs()
        {
            // Validate watch folder
            var watchValidation = PathValidator.ValidateSourcePath(txtWatchFolder.Text);
            if (!watchValidation.IsValid)
            {
                MessageBox.Show($"Watch Folder Error: {watchValidation.Message}", 
                    "Invalid Watch Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtWatchFolder.Focus();
                return false;
            }

            // Validate destination folder
            var destValidation = PathValidator.ValidateDestinationPath(txtDestinationPath.Text);
            if (!destValidation.IsValid)
            {
                MessageBox.Show($"Destination Folder Error: {destValidation.Message}", 
                    "Invalid Destination Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDestinationPath.Focus();
                return false;
            }

            // Validate file extensions
            if (string.IsNullOrWhiteSpace(txtFileExtensions.Text))
            {
                MessageBox.Show("File extensions cannot be empty. Please specify at least one extension (e.g., *.mp4)", 
                    "Invalid File Extensions", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFileExtensions.Focus();
                return false;
            }

            // Check if paths are the same
            if (string.Equals(Path.GetFullPath(txtWatchFolder.Text), 
                Path.GetFullPath(txtDestinationPath.Text), StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Watch folder and destination folder cannot be the same.", 
                    "Invalid Configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void ValidateWatchFolder()
        {
            var validation = PathValidator.ValidateSourcePath(txtWatchFolder.Text);
            if (!validation.IsValid)
            {
                lblWatchFolderStatus.Text = $"⚠ {validation.Message}";
                lblWatchFolderStatus.ForeColor = Color.Red;
            }
            else
            {
                lblWatchFolderStatus.Text = "✓ Valid watch folder";
                lblWatchFolderStatus.ForeColor = Color.Green;
            }
        }

        private void ValidateDestinationFolder()
        {
            var validation = PathValidator.ValidateDestinationPath(txtDestinationPath.Text);
            if (!validation.IsValid)
            {
                lblDestinationStatus.Text = $"⚠ {validation.Message}";
                lblDestinationStatus.ForeColor = Color.Red;
            }
            else
            {
                lblDestinationStatus.Text = "✓ Valid destination folder";
                lblDestinationStatus.ForeColor = Color.Green;
            }
        }

        private void txtWatchFolder_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtWatchFolder.Text))
            {
                ValidateWatchFolder();
            }
            else
            {
                lblWatchFolderStatus.Text = "";
            }
        }

        private void txtDestinationPath_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtDestinationPath.Text))
            {
                ValidateDestinationFolder();
            }
            else
            {
                lblDestinationStatus.Text = "";
            }
        }

        private void btnTestConfiguration_Click(object sender, EventArgs e)
        {
            if (!ValidateAllInputs())
                return;

            try
            {
                // Test by checking for existing files in watch folder
                string[] extensions = FileExtensions.Split(';', StringSplitOptions.RemoveEmptyEntries);
                List<string> foundFiles = new List<string>();

                foreach (string ext in extensions)
                {
                    string searchPattern = ext.Trim();
                    if (!searchPattern.StartsWith("*"))
                        searchPattern = "*" + searchPattern;

                    string[] files = Directory.GetFiles(txtWatchFolder.Text, searchPattern, SearchOption.TopDirectoryOnly);
                    foundFiles.AddRange(files.Take(5)); // Limit to first 5 files
                }

                string testResult = $"Configuration Test Results:\n\n";
                testResult += $"Watch Folder: {txtWatchFolder.Text}\n";
                testResult += $"Destination: {txtDestinationPath.Text}\n";
                testResult += $"Extensions: {txtFileExtensions.Text}\n";
                testResult += $"Stability Delay: {numStabilityDelay.Value} seconds\n\n";

                if (foundFiles.Any())
                {
                    testResult += $"Found {foundFiles.Count} existing files that would be monitored:\n";
                    foreach (string file in foundFiles)
                    {
                        string fileName = Path.GetFileName(file);
                        int tvConfidence = MediaTypeDetector.GetTVEpisodeConfidence(fileName);
                        testResult += $"• {fileName} (TV Confidence: {tvConfidence}%)\n";
                    }
                }
                else
                {
                    testResult += "No matching files found in watch folder currently.";
                }

                MessageBox.Show(testResult, "Configuration Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error testing configuration: {ex.Message}", 
                    "Test Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}