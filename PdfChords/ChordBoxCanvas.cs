using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Input;

namespace PdfChords
{    
    public class ChordBoxCanvas : Canvas
    {
        ChordBoxImage chord;

        char[] fingers = { 'x', 'x', 'x', 'x', 'x', 'x' };
        public string Chord()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(fingers[0]);
            for (int i = 1; i < fingers.Length; i++)
            {
                sb.Append(' ');
                sb.Append(fingers[i]);
            }
            return sb.ToString();
        }

        public ChordBoxCanvas() 
        {
            Width = 130;
            Height = 160;

            string str = new string(fingers);

            chord = new ChordBoxImage("", str, "------", "3");

            MouseLeftButtonDown += new MouseButtonEventHandler(ChordBoxCanvas_MouseLeftButtonDown);
        }

        void ChordBoxCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Windows.Point pos = Mouse.GetPosition(this);
            Console.WriteLine(pos.X + " " + pos.Y);

            int x = (int) Math.Round(pos.X) - 9;
            int y = (int) Math.Round(pos.Y) - 42;

            if (x >= 0 && x <= 72 && y >= 0 && y <= 78)
            {
                int n_string = x / 12;
                int n_fret = y / 13;

                if (fingers[n_string] == (char)('0' + n_fret))
                {
                    fingers[n_string] = 'x';
                }
                else
                {
                    fingers[n_string] = (char)('0' + n_fret);
                }

                /*
                if (n_fret == 0)
                {
                    if (fingers[n_string] == 'x') 
                    {
                        fingers[n_string] = '0';
                    }
                    else
                    {
                        fingers[n_string] = 'x';
                    }
                }
                else
                {
                    fingers[n_string] = (char)('0' + n_fret);
                }
                */
                  
                string str = new string (fingers);

                chord.Dispose();
                chord = new ChordBoxImage("", str, "------", "3");

                InvalidateVisual();
            }
                
        }

        protected override void OnRender(DrawingContext dc)
        {
            Bitmap bitmap = chord.GetBitmap();      
            int width = bitmap.Width;
            int height = bitmap.Height;

            BitmapSource bitmapSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(), 
                IntPtr.Zero, 
                System.Windows.Int32Rect.Empty, 
                BitmapSizeOptions.FromWidthAndHeight(width, height));

            dc.DrawImage(bitmapSrc, new Rect(0, 0, width, height));
        }
    }
}
