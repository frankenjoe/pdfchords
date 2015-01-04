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
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Diagnostics;
using Microsoft.Win32;

namespace PdfChords
{
    public class Error
    {
        public static void Show(string err)
        {
            Main.Log("Error: " + err);
            MessageBox.Show(err, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public partial class Main : Window
    {               
        XmlFileManager xmlFileManager = null;
        static StreamWriter logFile = null;    
        GridLength oldEditorWidth;
        GridLength oldBrowserWidth;

        public Main()
        {
            string appName = System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location);
            int appCount = System.Diagnostics.Process.GetProcessesByName(appName).Count();

            if (appCount > 1)
            {
                MessageBox.Show("An instance of '" + appName + "' is already running.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }

            InitializeComponent();

            this.Top = 0;
            this.Left = 0;

            xmlFileManager = new XmlFileManager(chordii);   
            logFile = new StreamWriter("PdfChords.log");
           
            chordii.ProcessOutput += new Chordii.ProcessOutputDelegate(chordii_ProcessOutputEvent);
            chordii.PropertyChanged += new PropertyChangedEventHandler(chordii_PropertyChanged);  
                 
            viewer.ButtonPrint.Click += new RoutedEventHandler(Viewer_ButtonPrint_Click);
            viewer.PrinterIndex = Properties.Settings.Default.printerIndex;

            editor.XmlExport = Properties.Settings.Default.xmlExport;
            editor.PsExport = Properties.Settings.Default.psExport;
            editor.PdfExport = Properties.Settings.Default.pdfExport;
            editor.AutoPreview = Properties.Settings.Default.autoPreview;            
            editor.ButtonPreview.Click += new RoutedEventHandler(Editor_ButtonPreview_Click);
            editor.ButtonNew.Click +=new RoutedEventHandler(Editor_ButtonNew_Click);
            editor.ButtonSave.Click += new RoutedEventHandler(Editor_ButtonSave_Click);
            editor.ButtonSaveAs.Click += new RoutedEventHandler(Editor_ButtonSaveAs_Click);
            editor.ButtonOpen.Click += new RoutedEventHandler(Editor_ButtonOpen_Click);
            editor.TextEditor.Drop += new DragEventHandler(Editor_TextEditor_Drop);
            editor.FinishEditEvent += new Editor_FinishEditDelegate(Editor_FinishEditEvent);
            editor.TextEditor.TextChanged += new EventHandler(Editor_TextEditor_TextChanged);
            editor.SetOwner(this);            
            editor_expander.Collapsed += new RoutedEventHandler(Editor_Collapsed);
            editor_expander.Expanded += new RoutedEventHandler(Editor_Expanded);

            browser.Directory = Properties.Settings.Default.directory;
            browser.ListBox.SelectionChanged += new SelectionChangedEventHandler(Browser_ListBox_SelectionChanged);                     
            browser_expander.Collapsed += new RoutedEventHandler(Browser_Collapsed);
            browser_expander.Expanded += new RoutedEventHandler(Browser_Expanded);                       

            this.KeyDown += new KeyEventHandler(Main_KeyDown);
            this.Closing += new System.ComponentModel.CancelEventHandler(Main_Closing);

            New();
        }

        #region mainevents

        public static void Log(string text)
        {
            logFile.Write(text);
            logFile.Flush();
        }

        void Main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!CheckIfFileHasChanged())
            {
                return;
            }

            Properties.Settings.Default.directory = browser.Directory;
            Properties.Settings.Default.printerIndex = viewer.PrinterIndex;
            Properties.Settings.Default.autoPreview = editor.AutoPreview;
            Properties.Settings.Default.xmlExport = editor.XmlExport;
            Properties.Settings.Default.psExport = editor.PsExport;
            Properties.Settings.Default.pdfExport = editor.PdfExport;
            Properties.Settings.Default.Save();

            logFile.Close();
        }        

        void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.P && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                Preview();
            }
            else if (e.Key == Key.F1)
            {
                MessageBox.Show("Author: " + Defines.AUTHOR + "\n\rVersion: " + Defines.VERSION + "\n\rEmail: " + Defines.EMAIL, Defines.TITLE, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion

        #region file

        string filepath = null;
        String Filepath
        {
            get { return filepath; }
            set { 
                filepath = value; 
                Title = Defines.TITLE + " " + Defines.VERSION + " " + (filepath == null ? "" : " (" + filepath + ")"); 
            }
        }

        bool fileHasChanged = false;
        public bool FileHasChanged
        {
            get { return fileHasChanged; }
            set { 
                if (value && !fileHasChanged) { 
                    Title += " *"; 
                } else if (!value && fileHasChanged)
                {
                    Title = Defines.TITLE + " " + Defines.VERSION + " " + (filepath == null ? "" : " (" + filepath + ")");
                }
                fileHasChanged = value; 
            }
        }

        bool CheckIfFileHasChanged()
        {
            if (FileHasChanged && editor.TextEditor.Text != "")
            {
                MessageBoxResult result = MessageBox.Show("Save changes?", "", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Save();
                        return true;
                    case MessageBoxResult.No:
                        return true;
                    case MessageBoxResult.Cancel:
                        return false;
                }
            }

            return true;
        }

        string ParseFilename(string text)
        {
            string suggestion = "";

            if (text.Length > 0)
            {

                Match t_match = Regex.Match(text, @"{(t|title):(.*)}");
                Match st_match = Regex.Match(text, @"{(st|subtitle):(.*)}");

                if (st_match.Success && st_match.Groups.Count > 2)
                {
                    suggestion += st_match.Groups[2];
                }                
                if (t_match.Success && t_match.Groups.Count > 2)
                {
                    if (suggestion != "")
                    {
                        suggestion += " - ";
                    }
                    suggestion += t_match.Groups[2];
                }
            }

            return suggestion.Trim();
        }

        #endregion

        #region editor

        void Editor_TextEditor_Drop(object sender, DragEventArgs e)
        {
            try
            {
                string[] filepath = (string[])e.Data.GetData(DataFormats.FileDrop);
                string content = File.ReadAllText(filepath[0]);
                editor.TextEditor.Text = content;

                if (editor.AutoPreview)
                {
                    Preview();
                }

            }
            catch (Exception ex)
            {
                Error.Show(ex.ToString());
            }
        }

        void Editor_FinishEditEvent()
        {
            if (editor.AutoPreview)
            {
                Preview();
            }
        }

        void Editor_Expanded(object sender, RoutedEventArgs e)
        {
            main.ColumnDefinitions[2].MinWidth = Defines.EDITOR_MIN_WIDTH;
            main.ColumnDefinitions[2].Width = oldEditorWidth;
        }

        void Editor_Collapsed(object sender, RoutedEventArgs e)
        {
            main.ColumnDefinitions[2].MinWidth = 0;
            oldEditorWidth = main.ColumnDefinitions[2].Width;
            main.ColumnDefinitions[2].Width = new GridLength(23);
        }

        void Editor_TextEditor_TextChanged(object sender, EventArgs e)
        {
            FileHasChanged = true;
        }

        void Editor_ButtonPreview_Click(object sender, RoutedEventArgs e)
        {
            Preview();
        }

        void Editor_ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            Open();
        }

        void Open()
        {
            if (CheckIfFileHasChanged())
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.DefaultExt = Defines.PRO_FILE_EXTENSION;
                dlg.Filter = "pro files (*.pro)|*.pro|All files (*.*)|*.*";
                dlg.CheckFileExists = true;
                if (dlg.ShowDialog() ?? false)
                {
                    Filepath = System.IO.Path.GetDirectoryName(dlg.FileName) + "\\" + System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
                    editor.TextEditor.Load(dlg.FileName);
                    FileHasChanged = false;
                }
            }
        }

        void Editor_ButtonSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        public void Save () {

            if (Filepath == null)
            {
                SaveAs();
            }
            else
            {
                File.WriteAllText(Filepath + Defines.PRO_FILE_EXTENSION, editor.TextEditor.Text);
                FileHasChanged = false;
                Export();
                browser.Update();
            }

            if (editor.AutoPreview)
            {
                Preview();
            }
        }
       
        void Editor_ButtonSaveAs_Click(object sender, EventArgs e)
        {
            SaveAs();
        }

        void SaveAs () {

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = Defines.PRO_FILE_EXTENSION;
            dlg.Filter = "pro files (*.pro)|*.pro|All files (*.*)|*.*";
            string suggest = ParseFilename(editor.TextEditor.Text);
            if (suggest != "")
            {
                dlg.FileName = suggest + Defines.PRO_FILE_EXTENSION;
            }
            if (dlg.ShowDialog() ?? false)
            {
                Filepath = System.IO.Path.GetDirectoryName(dlg.FileName) + "\\" + System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);         
            }
            else
            {
                return;
            }

            Save();
        }
        
        void Editor_ButtonNew_Click(object sender, EventArgs e)
        {
            New();
        }

        void New () {

            if (CheckIfFileHasChanged())
            {
                editor.TextEditor.Text = "{t:}\n{st:}\n";
                Filepath = null;
                FileHasChanged = false;
            }
        }

        #endregion

        #region browser

        void Browser_Expanded(object sender, RoutedEventArgs e)
        {
            main.ColumnDefinitions[0].MinWidth = Defines.BROWSER_MIN_WIDTH;
            main.ColumnDefinitions[0].Width = oldBrowserWidth;
        }

        void Browser_Collapsed(object sender, RoutedEventArgs e)
        {
            main.ColumnDefinitions[0].MinWidth = 0;
            oldBrowserWidth = main.ColumnDefinitions[0].Width;
            main.ColumnDefinitions[0].Width = new GridLength(23);
        }
       
        void Browser_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!CheckIfFileHasChanged ())
            {
                return;
            }

            try
            {
                string filename = (string)browser.ListBox.SelectedItem;
                if (filename != null)
                {
                    bool autoPreview_tmp = editor.AutoPreview;
                    editor.AutoPreview = false;

                    string propath = browser.Directory + "\\" + filename + Defines.PRO_FILE_EXTENSION;
                    editor.TextEditor.Text = File.ReadAllText(propath); ;

                    string xmlpath = browser.Directory + "\\" + filename + Defines.CHORDII_FILE_EXTENSION;
                    if (File.Exists(xmlpath))
                    {
                        xmlFileManager.Load(xmlpath);
                    }
                                        
                    editor.AutoPreview = autoPreview_tmp;
                    Filepath = browser.Directory + "\\" + filename;
                    FileHasChanged = false;                        
                    Preview();
                }
            }
            catch (Exception ex)
            {
                Error.Show(ex.ToString());
            }

        }                     

        #endregion

        #region viewer

        void Print()
        {
            string printer = viewer.PrinterSelection.SelectedValue.ToString();
            if (printer != null && printer != "")
            {
                chordii.PrintPs(printer);
            }
        }

        void Viewer_ButtonPrint_Click(object sender, RoutedEventArgs e)
        {
            Preview();
            Print();
        }


        #endregion

        #region chordii

        void Preview()
        {
            Cursor = Cursors.Wait;

            string inpath = "~" + Defines.PRO_FILE_EXTENSION;
            string pspath = "~" + Defines.PS_FILE_EXTENSION;
            string pdfpath = "~" + Defines.PDF_FILE_EXTENSION;

            File.WriteAllText(inpath, editor.TextEditor.Text);

            if (chordii.CreatePs(inpath, pspath))
            {
                int width = (int)this.MaxWidth;
                //chordii.View(outpath);
                if (chordii.CreatePdf(pspath, pdfpath))
                {
                    try
                    {
                        float zoom = viewer.MoonPdfPanel.CurrentZoom;
                        viewer.MoonPdfPanel.OpenFile(pdfpath);
                        if (viewer.MoonPdfPanel.IsLoaded)
                        {
                            viewer.MoonPdfPanel.Zoom(zoom);
                        }
                    }
                    catch (Exception e)
                    {
                        Error.Show(e.ToString());
                    }
                }
            }

            Cursor = Cursors.Arrow;
        }  

        void Export()
        {
            Cursor = Cursors.Wait;

            bool do_ps = editor.PsExport;
            bool do_pdf = editor.PdfExport;
            bool do_xml = editor.XmlExport;

            try
            {
                string path_pro = do_xml ? Filepath + Defines.PRO_FILE_EXTENSION : "~" + Defines.PRO_FILE_EXTENSION;
                string path_ps = do_ps ? Filepath + Defines.PS_FILE_EXTENSION : "~" + Defines.PS_FILE_EXTENSION;
                string path_pdf = Filepath + Defines.PDF_FILE_EXTENSION;

                if (do_ps || do_pdf || do_xml)
                {                    
                    if (do_xml)
                    {
                        xmlFileManager.Save(path_pro, editor.TextEditor.Text);                        
                    }

                    if (do_ps || do_pdf)
                    {
                        chordii.CreatePs(path_pro, path_ps);

                        if (do_pdf)
                        {
                            chordii.CreatePdf(path_ps, path_pdf);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Error.Show(e.ToString());
            }

            Cursor = Cursors.Arrow;
        }


        void chordii_ProcessOutputEvent(string output)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
            {
                Main.Log (output);
            }));
        }

        void chordii_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            FileHasChanged = true;
            if (editor.AutoPreview)
            {
                Preview();
            }
        }
       
        #endregion
        
    }
}
