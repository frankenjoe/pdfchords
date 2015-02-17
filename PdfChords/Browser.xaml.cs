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
using System.ComponentModel;

namespace PdfChords
{
    public partial class Browser : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Browser()
        {
            InitializeComponent();

            DataContext = this;
            browserList = new BindingList<string>();
        }

        void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        static BindingList<string> browserList = null;
        string directory = ".";
        public string Directory
        {
            get { return directory; }
            set { 
                if (System.IO.Directory.Exists (value)) {
                    directory = value;                    
                    OnPropertyChanged("Directory");
                    Update();
                }
            }
        }

        public void Add(string filename)
        {
            if (browserList.IndexOf(filename) == -1)
            {
                browserList.Add(filename);
                Update ();
                ListBox.SelectedItem = filename;
            }
        }

        void DeleteClick (object sender, RoutedEventArgs e)
        {
            if (ListBox.SelectedItem != null)
            {
                String name = (String)ListBox.SelectedItem;
                string[] files = System.IO.Directory.GetFiles(Directory, name + ".*");
                if (files.Length > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (String file in files)
                    {
                        sb.Append(file + "\r\n");
                    }
                    MessageBoxResult result = MessageBox.Show(sb.ToString(), "Delete Files?", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        foreach (String file in files)
                        {
                            File.Delete(file);
                        }
                    }
                }
                ListBox.SelectedItem = null;
            }
            Update ();
        }

        void UpdateClick(object sender, RoutedEventArgs e)
        {
            Update();
        }

        public void Update()
        {
            Update (ListBox.SelectedItem == null ? null : (string)ListBox.SelectedItem, Directory);
        }

        public void Update(string fileToSelect, string directory)
        {
            if (directory != Directory)
            {
                Directory = directory;
            }

            try
            {                               
                int selectedIndex = -1;

                string[] files = System.IO.Directory.GetFiles(Directory, "*" + Defines.PRO_FILE_EXTENSION);
                for (int i = 0; i < files.Length; i++)
                {
                    files[i] = System.IO.Path.GetFileNameWithoutExtension(files[i]);
                }
                Array.Sort(files);

                browserList.Clear();
                ListBox.ItemsSource = null;
                int count = 0;
                foreach (string file in files)
                {
                    if (file != "~")
                    {
                        browserList.Add(file);
                        if (file == fileToSelect)
                        {
                            selectedIndex = count;
                        }
                    }
                    count++;
                }
                ListBox.ItemsSource = browserList;
                if (selectedIndex >= 0)
                {
                    ListBox.SelectedIndex = selectedIndex;
                }
            }
            catch (Exception e)
            {
                Error.Show(e.ToString());
            }
        }

        void DirectoryTextChanged(object sender, TextChangedEventArgs e)
        {
            string text = ((TextBox)sender).Text;
            Directory = text;
        }

        void ChangeDirectoryClick(object sender, RoutedEventArgs e)
        {
            WPFFolderBrowser.WPFFolderBrowserDialog dialogue = new WPFFolderBrowser.WPFFolderBrowserDialog();
            dialogue.InitialDirectory = Directory;            
            if (dialogue.ShowDialog() == true)
            {
                Directory = dialogue.FileName;
            }
        }

    }
}
