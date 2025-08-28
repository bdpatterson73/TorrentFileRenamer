using System.Text.RegularExpressions;
using System.Diagnostics;
using static System.Windows.Forms.Design.AxImporter;
using System.Runtime.InteropServices;
using System.Text;
//using TMDbLib.Client;
//using TMDbLib.Objects.General;
//using TMDbLib.Objects.Authentication;
//using TMDbLib.Objects.Movies;
//using TMDbLib.Objects.Reviews;
//using TMDbLib.Objects.Search;
//using TMDbLib.Rest;
//using TMDbLib.Utilities;
//using Credits = TMDbLib.Objects.Movies.Credits;

namespace TorrentFileRenamer
{
    public partial class frmMain : Form
    {
        private static string videoFolder = "";
        const int MAX_PATH = 255;


        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetShortPathName([MarshalAs(UnmanagedType.LPTStr)] string path, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder shortPath,
       int shortPathLength
       );

        public frmMain()
        {
            InitializeComponent();
        }

        private void miScanFiles_Click(object sender, EventArgs e)
        {
            statusLabel.Text = "";
            statusProgress.Value = 0;
            frmScanOptions options = new frmScanOptions();
            DialogResult dr = options.ShowDialog();
            if (dr == DialogResult.OK)
            {

                string[] allFiles = Directory.GetFiles(options.VideoPath, "*.*", SearchOption.AllDirectories);
                foreach (string s in allFiles)
                {
                    if (s.EndsWith(options.FileExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        FileEpisode fe = new FileEpisode(s, options.OutputDirectory);
                        
                        // Only add files where we successfully parsed episode information
                        if (fe.EpisodeNumber != 0 && !string.IsNullOrWhiteSpace(fe.ShowName) && fe.ShowName != "Unknown Show")
                        {
                            ListViewItem lvi = new ListViewItem();
                            lvi.Text = s;
                            lvi.SubItems.Add(fe.NewFileNamePath);
                            lvi.SubItems.Add(fe.ShowName);
                            lvi.SubItems.Add(fe.SeasonNumber.ToString());
                            
                            // Display episode numbers - show all episodes if multi-episode
                            string episodeDisplay;
                            if (fe.IsMultiEpisode)
                            {
                                episodeDisplay = string.Join(", ", fe.EpisodeNumbers.OrderBy(x => x));
                            }
                            else
                            {
                                episodeDisplay = fe.EpisodeNumber.ToString();
                            }
                            lvi.SubItems.Add(episodeDisplay);
                            
                            lvi.SubItems.Add("");
                            lvi.Tag = fe;
                            lvFiles.Items.Add(lvi);
                        }
                        else
                        {
                            // Add unparsed files with a different background color for review
                            ListViewItem lvi = new ListViewItem();
                            lvi.Text = s;
                            lvi.SubItems.Add("UNPARSED - Please review");
                            lvi.SubItems.Add(fe.ShowName ?? "Unknown");
                            lvi.SubItems.Add(fe.SeasonNumber.ToString());
                            lvi.SubItems.Add(fe.EpisodeNumber.ToString());
                            lvi.SubItems.Add("Not processed");
                            lvi.BackColor = Color.LightGray;
                            lvi.Tag = fe;
                            lvFiles.Items.Add(lvi);
                        }
                    }
                }
            }
            else
                Debug.WriteLine("Dialog Cancelled");
        }


        private bool RetryFileMove(string sourceFile, string destFile, int NumRetries)
        {
            for (int i =0; i < NumRetries; i++)
            {
                try
                {
                    if (File.Exists(destFile))
                        File.Delete(destFile);
                    File.Copy(sourceFile, destFile);
                    return true;
                }
                catch (IOException iox)
                {
                    Debug.WriteLine("IO Exception. Waiting for file copy retry. " + i.ToString ());
                    System.Threading.Thread.Sleep(5000);
                }
            }
            return false;
        }


        /// <summary>
        /// Process TV Episodes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miProcess_Click(object sender, EventArgs e)
        {
            int listIndex = 0;
            statusProgress.Maximum = lvFiles.Items.Count;
            foreach (ListViewItem lvi in lvFiles.Items)
            {
                statusProgress.PerformStep();

                FileEpisode fe = (FileEpisode)lvi.Tag;
                lvFiles.EnsureVisible(listIndex);
                lvFiles.Refresh();
                Application.DoEvents();
                try
                {
                    if (!Directory.Exists(fe.NewDirectoryName))
                    {
                        Debug.WriteLine("No Directory. Creating: " + fe.NewDirectoryName);
                        Directory.CreateDirectory(fe.NewDirectoryName);
                    }

                    // Try to copy the file up to 5 times and watch for network communication problems.
                    bool retVal = RetryFileMove(fe.FullFilePath, fe.NewFileNamePath, 5);
                    if (retVal)
                    {
                        lvi.BackColor = Color.Yellow;
                        lvFiles.Refresh();
                        Application.DoEvents();
                    }
                    else
                    {
                        lvi.BackColor = Color.Red;
                        lvFiles.Refresh();
                    }
                }
                catch (Exception ex)
                {
                    lvi.BackColor = Color.Red;
                    lvFiles.Refresh();
                    Debug.WriteLine(ex.Message);
                }
                listIndex++;
                lvFiles.Refresh();
                Application.DoEvents();
            }
            listIndex = 0;
            // Loop trhough ALL the files and ensure local and remote match via CRC32.  Only when they match do we delete the local one.
            foreach (ListViewItem lvi in lvFiles.Items)
            {
                FileEpisode fe = (FileEpisode)lvi.Tag;
                lvFiles.EnsureVisible(listIndex);
                lvFiles.Refresh();
                Application.DoEvents();
                //string hashLocal = HashHelper.CRC32File(fe.FullFilePath);
                //string hashRemote = HashHelper.CRC32File(fe.NewFileNamePath);
                FileInfo fiLocal = new FileInfo(fe.FullFilePath);
                FileInfo fiRemote = new FileInfo(fe.NewFileNamePath);
                long sizeLocal = fiLocal.Length;
                long sizeRemote = fiRemote.Length;

                try
                {
                    //if (hashRemote == hashLocal)
                    
                        if (sizeLocal == sizeRemote)
                    {
                        File.Delete(fe.FullFilePath);
                            lvi.SubItems[5].Text = "Deleted";
                            Application.DoEvents();
                    }
                    else
                    {
                        lvi.SubItems[5].Text = "Source and destination don't match.";
                        Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    lvi.SubItems[5].Text = "Unable to delete source file.";
                    Application.DoEvents();
                }
                listIndex++;
            }
            statusLabel.Text = "Operation Completed at " + DateTime.Now.ToString();
        }

        private static string GetShortPath(string path)
        {
            var shortPath = new StringBuilder(MAX_PATH);
            GetShortPathName(path, shortPath, MAX_PATH);
            return shortPath.ToString();
        }

            public static bool DeleteFileWithRetry(string filename)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {

                    string did = GetShortPath(filename);
                    File.Delete(GetShortPath(filename));
                    return true;
                }
                catch (Exception ex)
                {
                    Thread.Sleep(1000);
                }
            }

            return false;
        }

        private void miRemove_Click(object sender, EventArgs e)
        {
            // Remove the currently selected listviewitem.
            foreach (ListViewItem lvi in lvFiles.SelectedItems)
            {
                try
                {
                    lvFiles.Items.Remove(lvi);
                }
                catch (Exception ex) { }
            }
            

        }

        private void tsbScan_Click(object sender, EventArgs e)
        {

            miScanFiles_Click(this, null);
        }

        private void tsbProcess_Click(object sender, EventArgs e)
        {
            miProcess_Click(this, null);
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lvFiles.Items.Clear();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

            //DateTime startAt = DateTime.Now;
            //string c = HashHelper.CRC32File("C:\\swimapps\\swimcardb\\data\\MAIN_old.DCT");
            //DateTime endAt = DateTime.Now;
            //TimeSpan ts = endAt - startAt;
            //MessageBox.Show($"CRC32: {c}    {ts.TotalSeconds}");


            //TODO TMDbClient client = new TMDbClient("0ed08c5515fa7214f7af38a5835360dd");
            //SearchContainer<SearchMovie> results = client.SearchMovieAsync("Black Adam").Result;

            //Console.WriteLine($"Got {results.Results.Count:N0} of {results.TotalResults:N0} results");
            //foreach (SearchMovie result in results.Results)
            //    Console.WriteLine(result.Title);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            frmScanMovies sm = new frmScanMovies();
            DialogResult dr = sm.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string[] allFiles = Directory.GetFiles(sm.VideoPath, "*.*", SearchOption.AllDirectories);
                foreach (string s in allFiles)
                {
                    if (s.EndsWith(sm.FileExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        MovieFile mv = new MovieFile(s, sm.OutputDirectory);
                        ListViewItem lvi = new ListViewItem();
                        lvi.Text = s;
                        lvi.SubItems.Add(mv.NewDestDirectory);
                        lvi.SubItems.Add(mv.MovieName);
                        lvi.SubItems.Add(mv.MovieYear);
                        lvi.SubItems.Add("");
                        lvi.Tag = mv;
                        lvMovies.Items.Add(lvi);
                    }
                }
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            DateTime startMove = DateTime.Now;
            bool existed = false;
            int listIndex = 0;
            statusProgress.Maximum = lvMovies.Items.Count;
            foreach (ListViewItem lvi in lvMovies.Items)
            {
                statusProgress.PerformStep();
                float li = listIndex + 1;
                float pct = (li / lvMovies.Items.Count) * 100;
                int pcti = (int)pct;
                statusLabel.Text = pcti.ToString() + " %";
                MovieFile fe = (MovieFile)lvi.Tag;
                lvMovies.EnsureVisible(listIndex);
                lvMovies.Refresh();
                Application.DoEvents();
                try
                {
                    if (!Directory.Exists(Path.GetDirectoryName(fe.NewDestDirectory)))
                    {
                        //Debug.WriteLine("No Directory. Creating: " + fe.NewDirectoryName);
                        Directory.CreateDirectory(Path.GetDirectoryName(fe.NewDestDirectory));
                    }
                    if (File.Exists(fe.NewDestDirectory))
                    {
                        File.Delete(fe.NewDestDirectory);
                        lvi.BackColor = Color.Blue;
                        existed = true;
                    }
                    File.Move(fe.FileNamePath, fe.NewDestDirectory);
                    //DEBUG  System.Threading.Thread.Sleep(30000);
                    if (!existed)
                    {
                        lvi.BackColor = Color.Yellow;
                    }
                    else
                    {
                        existed = false;
                    }

                    //lvi.SubItems[4].Text = "Successful";
                    lvMovies.Refresh();
                    Application.DoEvents();
                }
                catch (Exception ex)
                {
                    lvi.BackColor = Color.Red;
                    lvMovies.Refresh();
                    Debug.WriteLine(ex.Message);
                    lvi.SubItems[4].Text = ex.Message;
                }
                listIndex++;
                lvMovies.Refresh();
                Application.DoEvents();
            }
            //MessageBox.Show("Move operation has finished.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
            TimeSpan ts = DateTime.Now - startMove;

            statusLabel.Text = "Completed at " + DateTime.Now.ToString() + " (" + ts.Minutes.ToString() + ":" + ts.Seconds.ToString("D2") + ")";
        }

        private void msMovieRemove_Click(object sender, EventArgs e)
        {
            try
            {
                lvMovies.Items.Remove(lvMovies.SelectedItems[0]);
            }
            catch (Exception ex) { }
        }

        private void msMovieClear_Click(object sender, EventArgs e)
        {
            lvMovies.Items.Clear();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            frmScanMovies sm = new frmScanMovies();
            DialogResult dr = sm.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string[] allFiles = Directory.GetFiles(sm.VideoPath, "*.*", SearchOption.AllDirectories);
                foreach (string s in allFiles)
                {
                    if (s.EndsWith(sm.FileExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        MovieFile mv = new MovieFile(s, sm.OutputDirectory);
                        if (!DoesTitleContainYear(mv.MovieName))
                        {
                            ListViewItem lvi = new ListViewItem();
                            lvi.Text = s;
                            lvi.SubItems.Add(mv.MovieName);
                            string mYear = GetMovieYear(mv.MovieName);
                            lvi.SubItems.Add(mYear);
                            lvi.SubItems.Add(mv.MovieName.Trim() + " (" + mYear.Trim() + ")" + Path.GetExtension(mv.FileNamePath));
                            lvi.SubItems.Add("");

                            lvi.Tag = mv;
                            if (mYear.Length > 0)
                                lvMovieCleaner.Items.Add(lvi);
                            else
                            {
                                lvi.BackColor = Color.Gray;
                                lvMovieCleaner.Items.Add(lvi);
                            }
                        }
                    }
                }
            }
        }

        private string GetMovieYear(string movieName)
        {
            //TMDbClient client = new TMDbClient("0ed08c5515fa7214f7af38a5835360dd");
            //SearchContainer<SearchMovie> results = client.SearchMovieAsync(movieName).Result;

            // Console.WriteLine($"Got {results.Results.Count:N0} of {results.TotalResults:N0} results");
            // foreach (SearchMovie result in results.Results)
            // {
            //    DateTime? dtRelease = result.ReleaseDate;
            //    if (dtRelease.HasValue)
            //    {
            //        DateTime dt = dtRelease.Value;
            //        return dt.Year.ToString();
            //    }
            //}
            return "";

        }

        //private SearchContainer<SearchMovie> GetMovieRecords(string movieName)
        //{
        //    TMDbClient client = new TMDbClient("0ed08c5515fa7214f7af38a5835360dd");
        //   return  client.SearchMovieAsync(movieName).Result;
        //}

        private bool DoesTitleContainYear(string movieName)
        {
            string pattern = @"\(([0-9]{4})\)";
            Regex re = new Regex(pattern);
            string txtYear = string.Empty;

            foreach (Match m in re.Matches(movieName))
            {
                return true;
                txtYear = m.Value;
                Console.Write(txtYear);
            }
            //if (movieName.Contains("C"))
            // {
            //    if (movieName.Contains(""))
            // }
            return false;

        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in lvMovieCleaner.Items)
            {

                string path = Path.GetDirectoryName(lvi.SubItems[0].Text);
                string newFile = Path.Combine(path, lvi.SubItems[3].Text);
                if (File.Exists(newFile))
                {
                    File.Delete(lvi.SubItems[0].Text);
                }
                else
                {
                    File.Move(lvi.SubItems[0].Text, newFile);
                }
                lvi.BackColor = Color.Yellow;
            }
        }

        private void tsbForceDateCheck_Click(object sender, EventArgs e)
        {
            //frmScanMovies sm = new frmScanMovies();
            //DialogResult dr = sm.ShowDialog();
            //if (dr == DialogResult.OK)
            //{
            //    string[] allFiles = Directory.GetFiles(sm.VideoPath, "*.*", SearchOption.AllDirectories);
            //    foreach (string s in allFiles)
            //    {
            //        if (s.EndsWith(sm.FileExtension, StringComparison.OrdinalIgnoreCase))
            //        {
            //            MovieFile mv = new MovieFile(s, sm.OutputDirectory);
            //            ListViewItem lvi = new ListViewItem();
            //            lvi.Text = s;
            //            lvi.SubItems.Add(mv.NewDestDirectory);
            //            lvi.SubItems.Add(mv.MovieName);
            //            lvi.SubItems.Add(mv.MovieYear);
            //            lvi.SubItems.Add("");
            //            MovieYearCheck myc = new MovieYearCheck();
            //            myc.movieFile = mv;
            //            //myc.movieSearchContainer = GetMovieRecords(mv.MovieName);
            //            lvi.Tag = myc;
            //            lvMovies.Items.Add(lvi);
            //        }
            //    }
            //}
        }
    }
}