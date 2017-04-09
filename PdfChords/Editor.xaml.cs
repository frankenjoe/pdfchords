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
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using System.Xml;
using FindReplace;
using Microsoft.Win32;
using System.ComponentModel;
using System.Text.RegularExpressions;
using ICSharpCode.AvalonEdit.Document;

namespace PdfChords
{
    public delegate void Editor_FinishEditDelegate(); 

    public partial class Editor : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public FindReplaceMgr FindReplacePopup = null;
        public Editor_FinishEditDelegate FinishEditEvent;

        Main main = null;        

        public Editor()
        {
            InitializeComponent();

            DataContext = this;

            TextEditor.PreviewDragEnter += new DragEventHandler(TextEditor_OnDragOver);
            TextEditor.PreviewDragOver += new DragEventHandler(TextEditor_OnDragOver);
            TextEditor.KeyDown += new KeyEventHandler(TextEditor_KeyDown);
            TextEditor.TextArea.TextEntered += new TextCompositionEventHandler(TextArea_TextEntered);

            ContextMenuChord.Click += new RoutedEventHandler(ContextMenuChord_Click);
            ContextMenuTitle.Click += new RoutedEventHandler(ContextMenuTitle_Click);
            ContextMenuSubTitle.Click += new RoutedEventHandler(ContextMenuSubTitle_Click);
            ContextMenuChorus.Click += new RoutedEventHandler(ContextMenuChorus_Click);
            ContextMenuTabular.Click += new RoutedEventHandler(ContextMenuTabular_Click);
            ContextMenuComment.Click += new RoutedEventHandler(ContextMenuComment_Click);
            ContextMenuConvertToPro.Click += new RoutedEventHandler(ContextMenuConvertToPro_Click);

            // Load our custom highlighting definition
            IHighlightingDefinition customHighlighting;
            using (Stream s = typeof(Editor).Assembly.GetManifestResourceStream("PdfChords.ChordProHighlighting.xshd"))
            {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (XmlReader reader = new XmlTextReader(s))
                {
                    customHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                        HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            HighlightingManager.Instance.RegisterHighlighting("Custom Highlighting", new string[] { ".cool" }, customHighlighting);
            TextEditor.SyntaxHighlighting = customHighlighting;

            // find and replace dialog;
            FindReplacePopup = new FindReplaceMgr();

            FindReplacePopup.CurrentEditor = new FindReplace.TextEditorAdapter(TextEditor);
            FindReplacePopup.ShowSearchIn = false;

            CommandBindings.Add(FindReplacePopup.FindBinding);
            CommandBindings.Add(FindReplacePopup.ReplaceBinding);
            CommandBindings.Add(FindReplacePopup.FindNextBinding);
        }
        
        void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
       
        internal void SetOwner(Main main)
        {
            this.main = main;
            FindReplacePopup.OwnerWindow = main;
        }

        #region contextmenu

        void ContextMenuChord_Click(object sender, RoutedEventArgs e)
        {            
            InsertChordBox();
        }

        void ContextMenuTitle_Click(object sender, RoutedEventArgs e)
        {
            WrapText("{t:", "}");
        }

        void ContextMenuSubTitle_Click(object sender, RoutedEventArgs e)
        {
            WrapText("{st:", "}");
        }

        void ContextMenuComment_Click(object sender, RoutedEventArgs e)
        {            
            WrapText("{c:", "}");
        }

        void ContextMenuTabular_Click(object sender, RoutedEventArgs e)
        {
            WrapText("{sot}\n", "\n{eot}");
        }

        void ContextMenuChorus_Click(object sender, RoutedEventArgs e)
        {
            WrapText("{soc}\n", "\n{eoc}");
        }

        void ContextMenuConvertToPro_Click(object sender, RoutedEventArgs e)
        {
            ConvertPRO();
        }
        
        #endregion

        #region buttons

        void ButtonInsertTitle_Click(object sender, RoutedEventArgs e)
        {
            WrapText("{t:", "}");
        }

        void ButtonInsertSubtitle_Click(object sender, RoutedEventArgs e)
        {
            WrapText("{st:", "}");
        }

        void ButtonInsertChorus_Click(object sender, RoutedEventArgs e)
        {
            WrapText("{soc}\n", "\n{eoc}");
        }

        void ButtonInsertTabular_Click(object sender, RoutedEventArgs e)
        {
            WrapText("{sot}\n", "\n{eot}");
        }

        void ButtonInsertComment_Click(object sender, RoutedEventArgs e)
        {
            WrapText("{c:", "}");
        }

        #endregion

        #region edit    

        void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text[e.Text.Length - 1] == '[')
            {
                InsertText("]");
                TextEditor.TextArea.Caret.Offset--;
                InsertChord();
            }
        }

        void InsertChordBox() 
        {
            ChordBoxWindow box = new ChordBoxWindow();
            box.ShowDialog();
            if (box.DialogResult == true)
            {
                InsertText(box.Chord());
            }
            box = null;
        }
 
        void InsertChord()
        {
            ChordCompletionWindow completion = new ChordCompletionWindow(TextEditor.TextArea, TextEditor.Text);
            completion.Show();
            completion.Closed += delegate
            {
                completion = null;
            };
        }

        void InsertText(string text)
        {
            int selectionIndex = TextEditor.SelectionStart;
            TextEditor.Document.Insert(selectionIndex, text);
            TextEditor.SelectionStart = selectionIndex + text.Length;
        }

        void WrapText(string before, string after)
        {
            int from = TextEditor.SelectionStart;
            int to = from + TextEditor.SelectionLength + before.Length;
            TextEditor.Document.Insert(from, before);
            TextEditor.Document.Insert(to, after);
            TextEditor.SelectionStart = from;
        }

        void ConvertPRO()
        {
            try
            {
                string crd = TextEditor.Text;
                string pro = Crd2Pro.Parse(crd);
                if (pro != "")
                {
                    TextEditor.Text = pro;
                }
            }
            catch (Exception ex)
            {
                Error.Show("Sorry folk, converting to pro format failed.\n\n" + ex.ToString());
            }
        }

        #endregion

        #region texteditor

        void TextEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                main.Save();
            }
            else if (e.Key == Key.Z && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (TextEditor.CanUndo)
                {
                    TextEditor.Undo();
                }
            }
            else if (e.Key == Key.Y && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (TextEditor.CanRedo)
                {
                    TextEditor.Redo();
                }
            }
            else if (e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                FindReplacePopup.ShowAsFind();
            } 
            else if (e.Key == Key.R && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                FindReplacePopup.ShowAsReplace();
            } 
            else if (e.Key == Key.OemPlus && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                TextEditor.FontSize++;
            } 
            else if (e.Key == Key.OemMinus && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                TextEditor.FontSize--;
            }
        }

        private void TextEditor_OnDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
            e.Handled = true;
        }

        #endregion

        #region toolbar              

        private void findClick(object sender, RoutedEventArgs e)
        {
            FindReplacePopup.ShowAsReplace();
        }

        bool chordMode;
        public bool ChordMode
        {
            get { return chordMode; }
            set
            {
                chordMode = value;
                OnPropertyChanged("ChordMode");
            }
        }

        bool xmlExport;
        public bool XmlCreate
        {
            get { return xmlExport; }
            set
            {
                xmlExport = value;
                OnPropertyChanged("XmlExport");
            }
        }

        bool psExport;
        public bool PsCreate
        {
            get { return psExport; }
            set
            {
                psExport = value;
                OnPropertyChanged("PsExport");
            }
        }

        bool pdfExport;
        public bool PdfCreate
        {
            get { return pdfExport; }
            set
            {
                pdfExport = value;
                OnPropertyChanged("PdfExport");
            }
        }

        bool autoPreview;
        public bool AutoPreview
        {
            get { return autoPreview; }
            set
            {
                autoPreview = value;
                OnPropertyChanged("AutoPreview");
            }
        }

        #endregion



    }
}
