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

namespace PdfChords
{
    /// <summary>
    /// Interaction logic for InputBox.xaml
    /// </summary>
    public partial class InputBox : Window
    {
        public class Parameters
        {
            public Parameters()
            {
                Info = "";
                Input = "";
                Text = "";
            }

            public string Info { get; set; }
            public string Input { get; set; }
            public string Text { get; set; }
        }

        Parameters param;

        public InputBox(ref Parameters param)
        {
            InitializeComponent();

            this.param = param;

            InfoLabel.Content = param.Info;
            InputLabel.Content = param.Input;
            InputTextBox.Text = param.Text;

            OkButton.Click += OkButton_Click;
            CancelButton.Click += CancelButton_Click;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            param.Text = InputTextBox.Text;
            Close();
        }
    }
}
