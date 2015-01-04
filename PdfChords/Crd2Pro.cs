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
using System.Text.RegularExpressions;

namespace PdfChords
{
    public class Crd2Pro
    {
        public static string Parse(string crd)
        {
            string[] lines = crd.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            int n = lines.Length;
            int i = 0;
            StringBuilder pro = new StringBuilder ();

            while (i < n)
            {
                if (i + 1 < n && isChordLine(lines[i]) && !isChordLine(lines[i + 1]))
                {
                    pro.AppendLine(integrateChords(lines[i], lines[i + 1]));
                    i += 2;
                }
                else
                {
                    pro.AppendLine(lines[i]);
                    i += 1;
                }
            }

            return pro.ToString ();
        }

        private static string integrateChords(string chords, string text)
        {            
            Regex rx = new Regex(@"\S+");
            int last = 0;
            StringBuilder result = new StringBuilder();

            int n_chords = chords.Length;
            int n_text = text.Length;
            while (n_chords > n_text)
            {
                text += " ";
                n_text++;
            }

            foreach (Match match in rx.Matches(chords))
            {
                int pos = match.Index;
                string insert = "[" + match.Value + "]";

                result.Append(text.Substring(last, pos - last));
                result.Append(insert);

                last = pos;
            }
            result.Append(text.Substring(last));

            return result.ToString ();
        }

        private static bool isChordLine(string line)
        {           
            string[] tokens = Regex.Split (line, @"\s+");
            int n = tokens.Length;

            bool result = false;
            for (int i = 0; i < n; i++) {
                if (tokens[i] != "")
                {
                    result = Regex.IsMatch(tokens[i].ToUpper(), @"^[CDEFGABH]{1}[#B]?[M]?(SUS)?[245679]?(/[CDEFGABH]{1}[#B])?$", RegexOptions.Compiled);
                    if (!result)
                    {
                        break;
                    }
                }
            }

            return result;
        }
    }

    
}
