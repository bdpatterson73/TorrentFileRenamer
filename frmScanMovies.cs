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
    public partial class frmScanMovies : Form
    {
        public frmScanMovies()
        {
            InitializeComponent();
        }

        private void frmScanMovies_Load(object sender, EventArgs e)
        {
            txtVideoPath.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        }

        public string VideoPath
        {
            get
            {
                return txtVideoPath.Text;
            }
        }

        public string FileExtension
        {
            get
            {
                return txtExtension.Text;
            }
        }

        public string OutputDirectory
        {
            get
            {
                return txtOutputDirectory.Text;
            }
        }

        private void btnBrowseVideoPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select the destination path";
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            fbd.ShowNewFolderButton = true;
            DialogResult dr = fbd.ShowDialog();
            if (dr == DialogResult.OK)
            {
                txtVideoPath.Text = fbd.SelectedPath;
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select the destination path";
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            fbd.ShowNewFolderButton = true;
            DialogResult dr = fbd.ShowDialog();
            if (dr == DialogResult.OK)
            {
                txtOutputDirectory.Text = fbd.SelectedPath;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
