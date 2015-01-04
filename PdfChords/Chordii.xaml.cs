/* PDFChords

Author: Johannes Wagner <frankenjoe@hotmail.com>
Copyright (C) 2015
https://github.com/frankenjoe/pdfchords

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with this program. If not, see <http://www.gnu.org/licenses/>.
 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PdfChords
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Chordii : UserControl, INotifyPropertyChanged
    {
        public delegate void ProcessOutputDelegate(string output);
        public event ProcessOutputDelegate ProcessOutput;
        public event PropertyChangedEventHandler PropertyChanged;

        public Chordii()
        {
            InitializeComponent();

            DataContext = this;
        }

        void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #region lyrics

        int lyricsSizeIndex = 6;
        public int LyricsSizeIndex
        {
            get { return lyricsSizeIndex; }
            set { lyricsSizeIndex = value; OnPropertyChanged("LyricsSizeIndex"); }
        }
        string LyricsSizeArg()
        {
            return " -t " + fontSizes[lyricsSizeIndex];
        }

        int lyricsFontIndex = 0;
        public int LyricsFontIndex
        {
            get { return lyricsFontIndex; }
            set { lyricsFontIndex = value; OnPropertyChanged("LyricsFontIndex"); }
        }
        string LyricsFontArg()
        {
            return " -T " + Font2PSName(lyricsFontIndex, lyricsItalic, lyricsBold);
        }

        bool lyricsItalic = false;
        public bool LyricsItalic
        {
            get { return lyricsItalic; }
            set { lyricsItalic = value; OnPropertyChanged("LyricsItalic"); }
        }

        bool lyricsBold = false;
        public bool LyricsBold
        {
            get { return lyricsBold; }
            set { lyricsBold = value; OnPropertyChanged("LyricsBold"); }
        }

        bool lyricsSingle = true;
        public bool LyricsSingle
        {
            get { return lyricsSingle; }
            set { lyricsSingle = value; OnPropertyChanged("LyricsSingle"); }
        }
        string LyricsSingleArg()
        {
            return lyricsSingle ? " -a" : "";
        }

        #endregion

        #region chords

        int chordsSizeIndex = 4;
        public int ChordsSizeIndex
        {
            get { return chordsSizeIndex; }
            set { chordsSizeIndex = value; OnPropertyChanged("ChordsSizeIndex"); }
        }
        string ChordsSizeArg()
        {
            return " -c " + fontSizes[chordsSizeIndex];
        }

        int chordsFontIndex = 0;
        public int ChordsFontIndex
        {
            get { return chordsFontIndex; }
            set { chordsFontIndex = value; OnPropertyChanged("ChordsFontIndex"); }
        }
        string ChordsFontArg()
        {
            return " -C " + Font2PSName(chordsFontIndex, chordsItalic, chordsBold);
        }

        bool chordsItalic = false;
        public bool ChordsItalic
        {
            get { return chordsItalic; }
            set { chordsItalic = value; OnPropertyChanged("ChordsItalic"); }
        }

        bool chordsBold = false;
        public bool ChordsBold
        {
            get { return chordsBold; }
            set { chordsBold = value; OnPropertyChanged("ChordsBold"); }
        }

        string[] chordsTranspose = { "-5", "-4", "-3", "-2", "-1", "0", "1", "2", "3", "4", "5", "6" };
        public string[] ChordsTranspose
        {
            get { return chordsTranspose; }
        }
        int chordsTransposeIndex = 5;
        public int ChordsTransposeIndex
        {
            get { return chordsTransposeIndex; }
            set { chordsTransposeIndex = value; OnPropertyChanged("ChordsTransposeIndex"); }
        }
        string ChordsTransposeArg()
        {
            return " -x " + chordsTranspose[chordsTransposeIndex];
        }

        bool chordsShow = true;
        public bool ChordsShow
        {
            get { return chordsShow; }
            set { chordsShow = value; OnPropertyChanged("ChordsShow"); }
        }
        string ChordsShowArg()
        {
            return chordsShow ? "" : " -l";
        }

        #endregion

        #region grids

        string[] gridsSizes = { "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50" };
        public string[] GridsSizes
        {
            get { return gridsSizes; }
        }

        int gridsSizeIndex = 10;
        public int GridsSizeIndex
        {
            get { return gridsSizeIndex; }
            set { gridsSizeIndex = value; OnPropertyChanged("GridsSizeIndex"); }
        }
        string GridsSizeArg()
        {
            return " -s " + gridsSizes[gridsSizeIndex];
        }

        bool gridsShow = false;
        public bool GridsShow
        {
            get { return gridsShow; }
            set { gridsShow = value; OnPropertyChanged("GridsShow"); }
        }
        string GridsShowArg()
        {
            return gridsShow ? "" : " -G";
        }

        #endregion

        #region create

        bool RunProcess(string filepath, string arguments)
        {
            Process process = new Process();
            process.StartInfo.FileName = filepath;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.OutputDataReceived += new DataReceivedEventHandler(OutputDataReceived);

            if (ProcessOutput != null)
            {
                ProcessOutput(filepath + " " + arguments + Environment.NewLine);
            }
            else
            {
                Console.WriteLine(filepath + " " + arguments);
            }

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();
            }
            catch (Exception e)
            {
                Error.Show(e.ToString());
                return false;
            }

            return true;
        }

        public bool CreatePs(string inpath, string outpath)
        {
            string arguments = LyricsSizeArg() + LyricsFontArg() + LyricsSingleArg() + ChordsSizeArg() + ChordsFontArg() + ChordsShowArg() + ChordsTransposeArg() + GridsSizeArg() + GridsShowArg() + " -o \"" + outpath + "\" \"" + inpath + "\"";
            return RunProcess(@"chordii", arguments);
        }

        public bool CreatePdf(string path_ps, string path_pdf)
        {
            string arguments = "-q -dBATCH -dNOPAUSE -sDEVICE=pdfwrite -sOutputFile=\"" + path_pdf + "\" \"" + path_ps + "\"";
            return RunProcess(@"gswin32c.exe", arguments);
        }

        public bool PrintPs(string printer)
        {
            string arguments = string.Format("-grey -noquery -printer \"" + printer + "\" \"{0}\"", "~.ps");
            return RunProcess(@"gsprint.exe", arguments);
        }

        void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (ProcessOutput != null)
            {
                ProcessOutput(e.Data);
            }
            else
            {
                Console.Write(e.Data);
            }
        }

        #endregion

        #region fonts

        string Font2PSName(int index, bool italic, bool bold)
        {
            StringBuilder name = new StringBuilder();

            name.Append(Regex.Replace(fontNames[index].ToString(), @"\s", ""));
            if (italic)
            {
                name.Append(",Italic");
            }
            if (bold)
            {
                name.Append(",Bold");
            }

            return name.ToString();
        }

        List<FontFamily> fontNames = CreateFontsList();
        public List<FontFamily> FontNames
        {
            get { return fontNames; }
        }
        private static List<FontFamily> CreateFontsList()
        {
            List<FontFamily> fonts = new List<FontFamily>();

            fonts.Add(new FontFamily("Times New Roman"));
            fonts.Add(new FontFamily("Helvetica"));
            fonts.Add(new FontFamily("Courier New"));

            return fonts;

        }

        string[] fontSizes = { "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24" };
        public string[] FontSizes
        {
            get { return fontSizes; }
        }

        #endregion
    }
}
