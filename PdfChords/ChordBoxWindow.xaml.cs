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
using System.Windows.Shapes;
using System.ComponentModel;

namespace PdfChords
{
    /// <summary>
    /// Interaction logic for ChordBoxWindow.xaml
    /// </summary>
    public partial class ChordBoxWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        string[] baseFret = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11" };
        public string[] BaseFret
        {
            get { return baseFret; }
        }

        int baseFretIndex = 0;
        public int BaseFretIndex
        {
            get { return baseFretIndex; }
            set { baseFretIndex = value; OnPropertyChanged("BaseFretIndex"); }
        }

        public ChordBoxWindow()
        {
            InitializeComponent();

            DataContext = this;
            
            InsertButton.Click += new RoutedEventHandler(InsertButton_Click);
        }

        void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        void InsertButton_Click(object sender, RoutedEventArgs e)
        {            
            DialogResult = true;
            Close();
        }

        public string Chord()
        {
            return "{define " + Name.Text + " base-fret " + BaseFretIndex + " frets " + ChordBox.Chord() + "}";
        }
    }
}
