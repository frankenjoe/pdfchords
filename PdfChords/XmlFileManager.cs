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
using System.Xml;
using System.IO;
using System.Windows.Media;

namespace PdfChords
{    

    public class XmlFileManager
    {
        Chordii chordii;

        public XmlFileManager(Chordii chordii)
        {
            this.chordii = chordii;
        }

        public bool Save(string filepath, string content) 
        {
            try
            {
                string filename = System.IO.Path.GetFileName(filepath);
                string filename_noex = System.IO.Path.GetFileNameWithoutExtension(filepath);
                string outputDir = System.IO.Path.GetDirectoryName(filepath);

                XmlTextWriter xml = new XmlTextWriter(outputDir + "\\" + filename_noex + Defines.CHORDII_FILE_EXTENSION, null);
                xml.Formatting = Formatting.Indented;

                xml.WriteStartDocument();
                xml.WriteStartElement("chordii");

                    xml.WriteStartElement("file");
                        xml.WriteString(filename);
                    xml.WriteEndElement();

                    xml.WriteStartElement("lyrics");
                        xml.WriteStartElement("size");
                            xml.WriteString(chordii.FontSizes[chordii.LyricsSizeIndex]);
                        xml.WriteEndElement();
                        xml.WriteStartElement("font");
                            xml.WriteString(chordii.FontNames[chordii.LyricsFontIndex].ToString ());
                        xml.WriteEndElement();
                        xml.WriteStartElement("single");
                            xml.WriteString(chordii.LyricsSingle.ToString ());
                        xml.WriteEndElement();
                    xml.WriteEndElement();

                    xml.WriteStartElement("chords");
                        xml.WriteStartElement("size");
                            xml.WriteString(chordii.FontSizes[chordii.ChordsSizeIndex]);
                        xml.WriteEndElement();
                        xml.WriteStartElement("font");
                            xml.WriteString(chordii.FontNames[chordii.ChordsFontIndex].ToString ());
                        xml.WriteEndElement();
                        xml.WriteStartElement("show");
                            xml.WriteString(chordii.ChordsShow.ToString());
                        xml.WriteEndElement();
                        xml.WriteStartElement("transpose");
                            xml.WriteString(chordii.ChordsTranspose[chordii.ChordsTransposeIndex]);
                        xml.WriteEndElement();
                    xml.WriteEndElement();

                    xml.WriteStartElement("grids");
                        xml.WriteStartElement("size");
                            xml.WriteString(chordii.GridsSizes[chordii.GridsSizeIndex]);
                        xml.WriteEndElement();                    
                        xml.WriteStartElement("show");
                            xml.WriteString(chordii.GridsShow.ToString ());
                        xml.WriteEndElement();                    
                    xml.WriteEndElement();

                xml.WriteEndElement();
                xml.WriteEndDocument();
                xml.Close();

                return true;
            }
            catch (Exception e)
            {
                Error.Show(e.ToString());
            }

            return false;
        }

        public void Load(string filepath)
        {            
            try
            {
                XmlTextReader xml = new XmlTextReader(filepath);
                string outputDir = System.IO.Path.GetDirectoryName(filepath);

                xml.ReadStartElement("chordii");
                if (xml.IsStartElement())
                {
                    xml.ReadStartElement("file");
                    string filename = xml.ReadString();                   
                    xml.ReadEndElement();

                    xml.ReadStartElement("lyrics");
                        xml.ReadStartElement("size");
                            chordii.LyricsSizeIndex = Math.Max (0, Array.IndexOf(chordii.FontSizes, xml.ReadString()));                            
                        xml.ReadEndElement();
                        xml.ReadStartElement("font");
                            chordii.LyricsFontIndex = Math.Max(0, chordii.FontNames.IndexOf (new FontFamily(xml.ReadString())));                            
                        xml.ReadEndElement();
                        xml.ReadStartElement("single");
                            chordii.LyricsSingle = Boolean.Parse (xml.ReadString());
                        xml.ReadEndElement();
                    xml.ReadEndElement();

                    xml.ReadStartElement("chords");
                        xml.ReadStartElement("size");
                            chordii.ChordsSizeIndex = Math.Max (0, Array.IndexOf(chordii.FontSizes, xml.ReadString())); 
                        xml.ReadEndElement();
                        xml.ReadStartElement("font");
                            chordii.ChordsFontIndex = Math.Max(0, chordii.FontNames.IndexOf(new FontFamily(xml.ReadString())));                            
                        xml.ReadEndElement();
                        xml.ReadStartElement("show");
                            chordii.ChordsShow = Boolean.Parse(xml.ReadString());
                        xml.ReadEndElement();
                        xml.ReadStartElement("transpose");
                            chordii.ChordsTransposeIndex = Math.Max (0, Array.IndexOf(chordii.ChordsTranspose, xml.ReadString())); 
                        xml.ReadEndElement();
                    xml.ReadEndElement();

                    xml.ReadStartElement("grids");
                        xml.ReadStartElement("size");
                            chordii.GridsSizeIndex = Math.Max(0, Array.IndexOf(chordii.GridsSizes, xml.ReadString())); 
                        xml.ReadEndElement();
                        xml.ReadStartElement("show");
                            chordii.GridsShow = Boolean.Parse(xml.ReadString());
                        xml.ReadEndElement();
                    xml.ReadEndElement();                                        
                }

                xml.Close();                
            }
            catch (Exception e)
            {
                Error.Show(e.ToString());                
            }
        }
        
    }
}
