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
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using System.Text.RegularExpressions;

namespace PdfChords
{
    public class ChordCompletionWindow : CompletionWindow
    {
        static Regex regex = new Regex (@"\[([^]]*)\]", RegexOptions.Compiled);

        public ChordCompletionWindow(TextArea area, string text)
            : base(area)
        {
            Width = 100;

            HashSet<string> chords = new HashSet<string>();
            Match match = regex.Match(text);
            while (match.Success)
            {
                chords.Add(match.Groups[1].Value);
                match = match.NextMatch();
            }

            IList<ICompletionData> data = CompletionList.CompletionData;
            foreach (string chord in chords)
            {
                if (chord != "")
                {
                    data.Add(new ChordCompletionData(chord));
                }
            }
        }
    }
}
