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
using MoonPdfLib;
using MoonPdfLib.MuPdf;
using System.ComponentModel;

namespace PdfChords
{
    /// <summary>
    /// Interaction logic for Viewer.xaml
    /// </summary>
    public partial class Viewer : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal MoonPdfPanel MoonPdfPanel { get { return this.moonPdfPanel; } }

        public Viewer()
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

        int printerIndex = 0;
        public int PrinterIndex
        {
            get { return printerIndex; }
            set { printerIndex = value; OnPropertyChanged("PrinterIndex"); }
        }

        void ZoomOutClick(object sender, RoutedEventArgs e)
        {
            moonPdfPanel.ZoomOut();
        }

        void ZoomInClick(object sender, RoutedEventArgs e)
        {
            moonPdfPanel.ZoomIn();
        }
    }
}
