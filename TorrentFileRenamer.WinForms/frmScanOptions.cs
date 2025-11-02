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
    public partial class frmScanOptions : Form
    {
        public frmScanOptions()
        {
            InitializeComponent();
        }

        private void frmScanOptions_Load(object sender, EventArgs e)
        {
            txtVideoPath.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            txtExtension.Text = ".mp4"; // Default extension
        }

        public string VideoPath
        {
            get { return txtVideoPath.Text; }
        }

        public string FileExtension
        {
            get
            {
                string ext = txtExtension.Text.Trim();
                return ext.StartsWith(".") ? ext : "." + ext;
            }
        }

        public string OutputDirectory
        {
            get { return txtOutputDirectory.Text; }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                this.DialogResult = DialogResult.OK;
                Close();
            }
        }

        private bool ValidateInputs()
        {
            // Validate source path
            var sourceValidation = PathValidator.ValidateSourcePath(txtVideoPath.Text);
            if (!sourceValidation.IsValid)
            {
                MessageBox.Show($"Source Path Error: {sourceValidation.Message}",
                    "Invalid Source Path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtVideoPath.Focus();
                return false;
            }

            // Validate destination path
            long estimatedSpace = PathValidator.EstimateRequiredSpace(txtVideoPath.Text, FileExtension);
            var destValidation = PathValidator.ValidateDestinationPath(txtOutputDirectory.Text, estimatedSpace);
            if (!destValidation.IsValid)
            {
                MessageBox.Show($"Destination Path Error: {destValidation.Message}",
                    "Invalid Destination Path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtOutputDirectory.Focus();
                return false;
            }

            // Validate file extension
            var extValidation = PathValidator.ValidateFileExtension(txtExtension.Text);
            if (!extValidation.IsValid)
            {
                MessageBox.Show($"File Extension Error: {extValidation.Message}",
                    "Invalid File Extension", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtExtension.Focus();
                return false;
            }

            // Check if paths are the same
            if (string.Equals(Path.GetFullPath(txtVideoPath.Text),
                    Path.GetFullPath(txtOutputDirectory.Text), StringComparison.OrdinalIgnoreCase))
            {
                var result = MessageBox.Show("Source and destination paths are the same. This will move files within the same directory structure. Continue?",
                    "Same Path Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                    return false;
            }

            // Show space information if available
            if (estimatedSpace > 0)
            {
                string spaceMessage = $"Estimated space required: {FormatBytes(estimatedSpace)}\n{destValidation.Message}";
                MessageBox.Show(spaceMessage, "Space Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            return true;
        }

        private string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;

            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }

            return string.Format("{0:n1} {1}", number, suffixes[counter]);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select the destination path for TV episodes";
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            fbd.ShowNewFolderButton = true;

            if (!string.IsNullOrWhiteSpace(txtOutputDirectory.Text) && Directory.Exists(txtOutputDirectory.Text))
            {
                fbd.SelectedPath = txtOutputDirectory.Text;
            }

            DialogResult dr = fbd.ShowDialog();
            if (dr == DialogResult.OK)
            {
                txtOutputDirectory.Text = fbd.SelectedPath;

                // Validate the selected path immediately
                var validation = PathValidator.ValidateDestinationPath(fbd.SelectedPath);
                if (!validation.IsValid)
                {
                    MessageBox.Show($"Warning: {validation.Message}",
                        "Path Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void btnBrowseVideoPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select the source path containing video files";
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            fbd.ShowNewFolderButton = true;

            if (!string.IsNullOrWhiteSpace(txtVideoPath.Text) && Directory.Exists(txtVideoPath.Text))
            {
                fbd.SelectedPath = txtVideoPath.Text;
            }

            DialogResult dr = fbd.ShowDialog();
            if (dr == DialogResult.OK)
            {
                txtVideoPath.Text = fbd.SelectedPath;

                // Validate the selected path immediately
                var validation = PathValidator.ValidateSourcePath(fbd.SelectedPath);
                if (!validation.IsValid)
                {
                    MessageBox.Show($"Warning: {validation.Message}",
                        "Path Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }
}