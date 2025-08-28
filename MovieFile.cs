using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TorrentFileRenamer
{
    internal class MovieFile
    {
        private string? _fileNamePath;
        private string _newParentRootDir;
        public MovieFile(string? fileNamePath, string newParentRootDir)
        {
            _fileNamePath = fileNamePath;
            _newParentRootDir = newParentRootDir;
            ProcessFile();
            
        }

        public string FileNamePath
        {
            get
            {
                return _fileNamePath;
            }
        }
        public string? FilePath { get; set; }
        public string? FileName { get; set; }

        public string? MovieName { get; set; }

        public string? MovieYear { get; set; }

        public string NewDestDirectory { get; set; }

        private void ProcessFile()
        {
            GetFilePath();
            GetFileName();
            GetNewParentDirectory();
            GetMovieName();
        }


        private void GetMovieName()
        {
            string extension = Path.GetExtension(_fileNamePath).ToUpper().Trim();
            string newfileName = FileName.Replace(extension, "",StringComparison.OrdinalIgnoreCase).Trim();
            newfileName = newfileName.Replace(".", " ").Replace("_", " ").Trim();
            bool year = DoesTitleContainYear(newfileName);
            if (year)
            {
                MovieName = StripDate(newfileName);
            }
            else
                MovieName = newfileName;
        }

        private void GetFilePath()
        {
            if (_fileNamePath != null)
            FilePath = Path.GetDirectoryName(_fileNamePath).Trim();
        }

        private void GetFileName()
        {
            if (FilePath != null)
            FileName = _fileNamePath.Replace(FilePath, "").Replace("\\", "");
        }

        private void GetNewParentDirectory()
        {
            // First clean up the filename...
            string newfileName = FileName.Replace(".", " ").Replace("_", " ").Trim();
            if (newfileName.StartsWith("The ",StringComparison.OrdinalIgnoreCase))
            {
                newfileName = newfileName.Substring(4,newfileName.Length - 4);
            }
            if (newfileName.StartsWith("A ", StringComparison.OrdinalIgnoreCase))
            {
                newfileName = newfileName.Substring(2, newfileName.Length - 2);
            }

            NewDestDirectory = newfileName[0].ToString().ToUpper();
            NewDestDirectory= Path.Combine(_newParentRootDir, NewDestDirectory);
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            FileName = textInfo.ToTitleCase(FileName);
            NewDestDirectory = Path.Combine(NewDestDirectory, FileName);
           
        }

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

        private string StripDate(string movieName)
        {
            string pattern = @"\(([0-9]{4})\)";
            Regex re = new Regex(pattern);
            string txtYear = string.Empty;
            string toReturn = movieName;
            foreach (Match m in re.Matches(movieName))
            {
                string yearToStrip =  m.Value ;
            this.MovieYear = m.Value;
              toReturn =  toReturn.Replace(yearToStrip,"").Trim();
                break;
            }

            return toReturn;
        }
    }
}
